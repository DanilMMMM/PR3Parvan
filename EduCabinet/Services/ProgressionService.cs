using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduCabinet.Data;
using EduCabinet.Models;

namespace EduCabinet.Services
{
    public class ProgressionService : IProgressionService
    {
        private readonly AppDbContext _context;

        public ProgressionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool CanAccess, string? Reason)> CheckLectureAccessAsync(int studentId, int lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Topic)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return (false, "Лекция не найдена.");
            }

            // Check if student has manual unlock or already completed
            var progress = await _context.LectureProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LectureId == lectureId);

            if (progress != null && (progress.IsUnlockedManually || progress.IsCompleted))
            {
                return (true, null);
            }

            // Get all lectures in this discipline ordered sequentially
            var disciplineLectures = await _context.Lectures
                .Include(l => l.Topic)
                .Where(l => l.Topic.DisciplineId == lecture.Topic.DisciplineId)
                .OrderBy(l => l.Topic.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            int currentIndex = disciplineLectures.FindIndex(l => l.Id == lectureId);

            // First lecture is always open to all students
            if (currentIndex <= 0)
            {
                if (progress == null)
                {
                    _context.LectureProgresses.Add(new LectureProgress
                    {
                        StudentId = studentId,
                        LectureId = lectureId,
                        IsCompleted = false,
                        IsUnlockedManually = false,
                        UnlockedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                return (true, null);
            }

            // Check previous lecture
            var prevLecture = disciplineLectures[currentIndex - 1];
            var prevProgress = await _context.LectureProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LectureId == prevLecture.Id);

            if (prevProgress != null && prevProgress.IsCompleted)
            {
                // Preceding lecture is completed! Unlock current lecture
                if (progress == null)
                {
                    _context.LectureProgresses.Add(new LectureProgress
                    {
                        StudentId = studentId,
                        LectureId = lectureId,
                        IsCompleted = false,
                        IsUnlockedManually = false,
                        UnlockedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                return (true, null);
            }

            return (false, $"Доступ к лекции закрыт. Чтобы перейти к ней, необходимо завершить изучение предыдущей лекции «{prevLecture.Title}» и успешно пройти проверочное тестирование (не менее {prevLecture.MinPassingScore}%).");
        }

        public async Task<bool> CompleteLectureAfterTestAsync(int studentId, int testId, int attemptId)
        {
            var attempt = await _context.TestAttempts
                .Include(a => a.Test)
                .ThenInclude(t => t.Lecture)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);

            if (attempt == null || !attempt.IsPassed)
            {
                return false;
            }

            var lectureId = attempt.Test.LectureId;
            var progress = await _context.LectureProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LectureId == lectureId);

            if (progress == null)
            {
                progress = new LectureProgress
                {
                    StudentId = studentId,
                    LectureId = lectureId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                _context.LectureProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CalculateDisciplineProgressPercentAsync(int studentId, int disciplineId)
        {
            var totalLectures = await _context.Lectures
                .Where(l => l.Topic.DisciplineId == disciplineId)
                .CountAsync();

            if (totalLectures == 0) return 0;

            var completedCount = await _context.LectureProgresses
                .Where(lp => lp.StudentId == studentId &&
                             lp.IsCompleted &&
                             lp.Lecture.Topic.DisciplineId == disciplineId)
                .CountAsync();

            return (int)Math.Round((double)completedCount / totalLectures * 100);
        }

        public async Task<List<LectureItemViewModel>> GetLecturesWithStatusForStudentAsync(int studentId, int disciplineId)
        {
            var lectures = await _context.Lectures
                .Include(l => l.Topic)
                .Include(l => l.Tests)
                .Where(l => l.Topic.DisciplineId == disciplineId)
                .OrderBy(l => l.Topic.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            var progresses = await _context.LectureProgresses
                .Where(lp => lp.StudentId == studentId && lp.Lecture.Topic.DisciplineId == disciplineId)
                .ToDictionaryAsync(lp => lp.LectureId);

            var attempts = await _context.TestAttempts
                .Where(ta => ta.StudentId == studentId)
                .GroupBy(ta => ta.Test.LectureId)
                .Select(g => new { LectureId = g.Key, BestScore = g.Max(x => x.ScorePercent) })
                .ToDictionaryAsync(x => x.LectureId, x => x.BestScore);

            var result = new List<LectureItemViewModel>();
            bool prevCompleted = true; // First lecture is open

            for (int i = 0; i < lectures.Count; i++)
            {
                var lec = lectures[i];
                progresses.TryGetValue(lec.Id, out var prog);
                attempts.TryGetValue(lec.Id, out var bestScore);

                bool isCompleted = prog?.IsCompleted ?? false;
                bool isUnlocked = (i == 0) || (prog?.IsUnlockedManually ?? false) || prevCompleted;

                result.Add(new LectureItemViewModel
                {
                    LectureId = lec.Id,
                    Title = lec.Title,
                    OrderIndex = lec.OrderIndex,
                    IsUnlocked = isUnlocked,
                    IsCompleted = isCompleted,
                    BestScorePercent = attempts.ContainsKey(lec.Id) ? bestScore : null,
                    HasTest = lec.Tests.Any()
                });

                prevCompleted = isCompleted;
            }

            return result;
        }

        public async Task<bool> ManuallyUnlockLectureAsync(int studentId, int lectureId, int teacherId, string reason)
        {
            var progress = await _context.LectureProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LectureId == lectureId);

            if (progress == null)
            {
                progress = new LectureProgress
                {
                    StudentId = studentId,
                    LectureId = lectureId,
                    IsUnlockedManually = true,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.LectureProgresses.Add(progress);
            }
            else
            {
                progress.IsUnlockedManually = true;
                progress.UnlockedAt = DateTime.UtcNow;
            }

            var student = await _context.Users.FindAsync(studentId);
            var lecture = await _context.Lectures.FindAsync(lectureId);

            _context.AuditLogs.Add(new AuditLog
            {
                TeacherId = teacherId,
                Action = "ManualUnlock",
                Details = $"Преподаватель открыл доступ к лекции «{lecture?.Title}» (Id={lectureId}) студенту {student?.FullName} (Id={studentId}). Причина: {reason}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetStudentTestAttemptsAsync(int studentId, int testId, int teacherId)
        {
            var attempts = await _context.TestAttempts
                .Where(ta => ta.StudentId == studentId && ta.TestId == testId)
                .ToListAsync();

            if (attempts.Any())
            {
                _context.TestAttempts.RemoveRange(attempts);

                var student = await _context.Users.FindAsync(studentId);
                var test = await _context.Tests.FindAsync(testId);

                _context.AuditLogs.Add(new AuditLog
                {
                    TeacherId = teacherId,
                    Action = "ResetAttempts",
                    Details = $"Преподаватель сбросил попытки прохождения теста «{test?.Title}» для студента {student?.FullName} (Id={studentId})",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
