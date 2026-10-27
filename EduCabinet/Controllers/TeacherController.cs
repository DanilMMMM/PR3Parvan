using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduCabinet.Data;
using EduCabinet.Models;
using EduCabinet.Services;

namespace EduCabinet.Controllers
{
    [Authorize(Roles = UserRoles.Teacher)]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProgressionService _progressionService;

        public TeacherController(AppDbContext context, IProgressionService progressionService)
        {
            _context = context;
            _progressionService = progressionService;
        }

        // GET: /Teacher/Index
        public async Task<IActionResult> Index()
        {
            var disciplines = await _context.Disciplines
                .Include(d => d.Topics)
                .ThenInclude(t => t.Lectures)
                .ToListAsync();

            var students = await _context.Users
                .Where(u => u.Role == UserRoles.Student)
                .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
                .ToListAsync();

            var groups = await _context.StudentGroups.ToListAsync();
            var totalAttempts = await _context.TestAttempts.CountAsync();
            var totalLectures = await _context.Lectures.CountAsync();

            var progressRows = new List<StudentProgressRow>();

            foreach (var student in students)
            {
                var groupName = student.GroupMemberships.FirstOrDefault()?.Group?.Name ?? "Без группы";
                var studentAttempts = await _context.TestAttempts
                    .Where(ta => ta.StudentId == student.Id)
                    .ToListAsync();

                var completedLecCount = await _context.LectureProgresses
                    .Where(lp => lp.StudentId == student.Id && lp.IsCompleted)
                    .CountAsync();

                double avgScore = studentAttempts.Any() ? studentAttempts.Average(a => a.ScorePercent) : 0;
                var lastActivity = studentAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt;

                int progressPct = totalLectures > 0 ? (int)Math.Round((double)completedLecCount / totalLectures * 100) : 0;

                progressRows.Add(new StudentProgressRow
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    GroupName = groupName,
                    DisciplineTitle = disciplines.FirstOrDefault()?.Title ?? "Дисциплина",
                    CompletedLectures = completedLecCount,
                    TotalLectures = totalLectures,
                    OverallProgressPercent = progressPct,
                    AverageScorePercent = Math.Round(avgScore, 1),
                    LastActivityAt = lastActivity
                });
            }

            var vm = new TeacherDashboardViewModel
            {
                TotalStudents = students.Count,
                TotalDisciplines = disciplines.Count,
                TotalLectures = totalLectures,
                TotalAttempts = totalAttempts,
                Disciplines = disciplines,
                Groups = groups,
                StudentsProgress = progressRows
            };

            return View(vm);
        }

        // GET: /Teacher/StudentDetails/5
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _context.Users
                .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRoles.Student);

            if (student == null) return NotFound();

            var disciplines = await _context.Disciplines
                .Include(d => d.Topics)
                .ThenInclude(t => t.Lectures)
                .ThenInclude(l => l.Tests)
                .ToListAsync();

            var attempts = await _context.TestAttempts
                .Include(ta => ta.Test)
                .ThenInclude(t => t.Lecture)
                .Where(ta => ta.StudentId == id)
                .OrderByDescending(ta => ta.CompletedAt)
                .ToListAsync();

            var progresses = await _context.LectureProgresses
                .Include(lp => lp.Lecture)
                .Where(lp => lp.StudentId == id)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.Attempts = attempts;
            ViewBag.Progresses = progresses;
            ViewBag.Disciplines = disciplines;

            return View();
        }

        // POST: /Teacher/ManualUnlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualUnlock(int studentId, int lectureId, string reason)
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _progressionService.ManuallyUnlockLectureAsync(studentId, lectureId, teacherId, reason ?? "Индивидуальное решение преподавателя");

            TempData["SuccessMessage"] = "Лекция успешно разблокирована для студента!";
            return RedirectToAction(nameof(StudentDetails), new { id = studentId });
        }

        // POST: /Teacher/ResetAttempts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAttempts(int studentId, int testId)
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _progressionService.ResetStudentTestAttemptsAsync(studentId, testId, teacherId);

            TempData["SuccessMessage"] = "Попытки тестирования успешно сброшены для студента.";
            return RedirectToAction(nameof(StudentDetails), new { id = studentId });
        }

        // GET: /Teacher/AuditLogs
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _context.AuditLogs
                .Include(al => al.Teacher)
                .OrderByDescending(al => al.Timestamp)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }

        // GET: /Teacher/Journal
        public async Task<IActionResult> Journal(int? disciplineId, int? groupId)
        {
            var disciplines = await _context.Disciplines
                .OrderBy(d => d.Title)
                .ToListAsync();

            if (!disciplines.Any())
            {
                return View(new AcademicJournalViewModel());
            }

            int selectedDisciplineId = disciplineId ?? disciplines.First().Id;

            var groups = await _context.StudentGroups
                .OrderBy(g => g.Name)
                .ToListAsync();

            var lectures = await _context.Lectures
                .Include(l => l.Topic)
                .Include(l => l.Tests)
                .Where(l => l.Topic.DisciplineId == selectedDisciplineId)
                .OrderBy(l => l.Topic.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            var studentsQuery = _context.Users
                .Where(u => u.Role == UserRoles.Student)
                .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
                .AsQueryable();

            if (groupId.HasValue)
            {
                studentsQuery = studentsQuery.Where(u => u.GroupMemberships.Any(gm => gm.GroupId == groupId.Value));
            }

            var students = await studentsQuery.OrderBy(u => u.FullName).ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();
            var lectureIds = lectures.Select(l => l.Id).ToList();
            var testIds = lectures.SelectMany(l => l.Tests).Select(t => t.Id).ToList();

            var progresses = await _context.LectureProgresses
                .Where(lp => studentIds.Contains(lp.StudentId) && lectureIds.Contains(lp.LectureId))
                .ToListAsync();

            var attempts = await _context.TestAttempts
                .Where(ta => studentIds.Contains(ta.StudentId) && testIds.Contains(ta.TestId))
                .ToListAsync();

            var matrixRows = new List<JournalStudentMatrixRow>();
            var detailEntries = new List<JournalDetailEntry>();
            int totalPassedCount = 0;

            var selectedDiscipline = disciplines.First(d => d.Id == selectedDisciplineId);

            foreach (var student in students)
            {
                var groupName = student.GroupMemberships.FirstOrDefault()?.Group?.Name ?? "Без группы";
                var studentProgresses = progresses.Where(p => p.StudentId == student.Id).ToDictionary(p => p.LectureId);
                var studentAttempts = attempts.Where(a => a.StudentId == student.Id).ToList();

                var row = new JournalStudentMatrixRow
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    GroupName = groupName,
                    TotalLectures = lectures.Count
                };

                int completedForStudent = 0;
                var studentScores = new List<int>();

                foreach (var lec in lectures)
                {
                    studentProgresses.TryGetValue(lec.Id, out var prog);
                    var test = lec.Tests.FirstOrDefault();
                    var lecAttempts = test != null
                        ? studentAttempts.Where(a => a.TestId == test.Id).OrderByDescending(a => a.ScorePercent).ToList()
                        : new List<TestAttempt>();

                    var bestAttempt = lecAttempts.FirstOrDefault();
                    int? bestScore = bestAttempt?.ScorePercent;
                    bool isPassed = bestAttempt?.IsPassed ?? false;
                    bool isCompleted = prog?.IsCompleted ?? false;
                    bool isManual = prog?.IsUnlockedManually ?? false;

                    int currentLecIndex = lectures.FindIndex(l => l.Id == lec.Id);
                    bool isUnlocked = isCompleted || isManual || (currentLecIndex == 0) ||
                                      (currentLecIndex > 0 && studentProgresses.TryGetValue(lectures[currentLecIndex - 1].Id, out var prevProg) && prevProg.IsCompleted);

                    if (isCompleted) completedForStudent++;
                    if (bestScore.HasValue) studentScores.Add(bestScore.Value);
                    if (isPassed) totalPassedCount++;

                    row.LectureCells[lec.Id] = new JournalCell
                    {
                        LectureId = lec.Id,
                        IsUnlocked = isUnlocked,
                        IsCompleted = isCompleted,
                        IsUnlockedManually = isManual,
                        ScorePercent = bestScore,
                        AttemptsCount = lecAttempts.Count,
                        LastAttemptDate = lecAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
                    };

                    string accessStatus = isCompleted ? "Освоена" : (isManual ? "Разблокирована вручную" : (isUnlocked ? "В процессе изучения" : "Заблокирована"));

                    detailEntries.Add(new JournalDetailEntry
                    {
                        StudentId = student.Id,
                        StudentFullName = student.FullName,
                        GroupName = groupName,
                        DisciplineTitle = selectedDiscipline.Title,
                        LectureTitle = lec.Title,
                        LectureOrderIndex = lec.OrderIndex,
                        AccessStatus = accessStatus,
                        ScorePercent = bestScore,
                        Grade5Scale = bestScore switch
                        {
                            >= 85 => "5 (Отлично)",
                            >= 70 => "4 (Хорошо)",
                            >= 50 => "3 (Удовл.)",
                            > 0 => "2 (Неуд.)",
                            _ => "—"
                        },
                        AttemptsCount = lecAttempts.Count,
                        MaxAttempts = test?.MaxAttempts ?? 3,
                        ActivityDate = lecAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt,
                        IsPassed = isPassed
                    });
                }

                row.CompletedCount = completedForStudent;
                row.ProgressPercent = lectures.Count > 0 ? (int)Math.Round((double)completedForStudent / lectures.Count * 100) : 0;
                row.AverageScorePercent = studentScores.Any() ? Math.Round(studentScores.Average(), 1) : 0;
                matrixRows.Add(row);
            }

            double avgPassRate = (students.Count > 0 && lectures.Count > 0)
                ? Math.Round((double)totalPassedCount / (students.Count * lectures.Count) * 100, 1)
                : 0;

            var vm = new AcademicJournalViewModel
            {
                SelectedDisciplineId = selectedDisciplineId,
                SelectedGroupId = groupId,
                Disciplines = disciplines,
                Groups = groups,
                DisciplineLectures = lectures,
                MatrixRows = matrixRows,
                DetailEntries = detailEntries.OrderBy(d => d.StudentFullName).ThenBy(d => d.LectureOrderIndex).ToList(),
                Stats = new JournalStats
                {
                    TotalStudentsCount = students.Count,
                    TotalLecturesCount = lectures.Count,
                    TotalPassedAttempts = totalPassedCount,
                    AveragePassRate = avgPassRate
                }
            };

            return View(vm);
        }

        // GET: /Teacher/LectureRoster?lectureId=5&groupId=1
        public async Task<IActionResult> LectureRoster(int lectureId, int? groupId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Topic)
                .ThenInclude(t => t.Discipline)
                .Include(l => l.Tests)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null) return NotFound();

            var groups = await _context.StudentGroups.OrderBy(g => g.Name).ToListAsync();

            var studentsQuery = _context.Users
                .Where(u => u.Role == UserRoles.Student)
                .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
                .AsQueryable();

            if (groupId.HasValue)
            {
                studentsQuery = studentsQuery.Where(u => u.GroupMemberships.Any(gm => gm.GroupId == groupId.Value));
            }

            var students = await studentsQuery.OrderBy(u => u.FullName).ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            var progresses = await _context.LectureProgresses
                .Where(lp => lp.LectureId == lectureId && studentIds.Contains(lp.StudentId))
                .ToDictionaryAsync(lp => lp.StudentId);

            var test = lecture.Tests.FirstOrDefault();
            var attempts = test != null
                ? await _context.TestAttempts
                    .Where(ta => ta.TestId == test.Id && studentIds.Contains(ta.StudentId))
                    .ToListAsync()
                : new List<TestAttempt>();

            // Determine sequential order for access determination
            var allLec = await _context.Lectures
                .Include(l => l.Topic)
                .Where(l => l.Topic.DisciplineId == lecture.Topic.DisciplineId)
                .OrderBy(l => l.Topic.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            int lecIndex = allLec.FindIndex(l => l.Id == lectureId);
            int? prevLecId = (lecIndex > 0) ? allLec[lecIndex - 1].Id : null;

            var prevProgresses = new Dictionary<int, LectureProgress>();
            if (prevLecId.HasValue)
            {
                prevProgresses = await _context.LectureProgresses
                    .Where(lp => lp.LectureId == prevLecId.Value && studentIds.Contains(lp.StudentId))
                    .ToDictionaryAsync(lp => lp.StudentId);
            }

            var studentRows = new List<LectureRosterStudentRow>();
            int completedCount = 0;
            int inProgressCount = 0;
            int lockedCount = 0;

            foreach (var student in students)
            {
                progresses.TryGetValue(student.Id, out var prog);
                var studentAttempts = attempts.Where(a => a.StudentId == student.Id).ToList();
                var bestAttempt = studentAttempts.OrderByDescending(a => a.ScorePercent).FirstOrDefault();
                int? bestScore = bestAttempt?.ScorePercent;
                var completedDate = prog?.CompletedAt ?? bestAttempt?.CompletedAt;

                bool isCompleted = prog?.IsCompleted ?? false;
                bool isManual = prog?.IsUnlockedManually ?? false;
                bool prevCompleted = !prevLecId.HasValue || (prevProgresses.TryGetValue(student.Id, out var prevProg) && prevProg.IsCompleted);
                bool isUnlocked = isCompleted || isManual || prevCompleted;

                string statusDisplay;
                if (isCompleted)
                {
                    statusDisplay = "Освоено (Зачтено)";
                    completedCount++;
                }
                else if (isUnlocked || isManual)
                {
                    statusDisplay = isManual ? "Открыто вручную (В процессе)" : "Доступно (В процессе)";
                    inProgressCount++;
                }
                else
                {
                    statusDisplay = "Заблокировано (Требуется сдача предыдущей лекции)";
                    lockedCount++;
                }

                studentRows.Add(new LectureRosterStudentRow
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    GroupName = student.GroupMemberships.FirstOrDefault()?.Group?.Name ?? "Без группы",
                    IsUnlocked = isUnlocked,
                    IsCompleted = isCompleted,
                    IsUnlockedManually = isManual,
                    AttemptsCount = studentAttempts.Count,
                    BestScorePercent = bestScore,
                    CompletedDate = completedDate,
                    StatusDisplay = statusDisplay,
                    Grade5Scale = bestScore switch
                    {
                        >= 85 => "5 (Отлично)",
                        >= 70 => "4 (Хорошо)",
                        >= 50 => "3 (Удовл.)",
                        > 0 => "2 (Неуд.)",
                        _ => "—"
                    }
                });
            }

            double passRate = students.Count > 0 ? Math.Round((double)completedCount / students.Count * 100, 1) : 0;

            var vm = new LectureRosterViewModel
            {
                Lecture = lecture,
                Discipline = lecture.Topic.Discipline,
                SelectedGroupId = groupId,
                Groups = groups,
                StudentRows = studentRows,
                TotalEnrolled = students.Count,
                TotalCompleted = completedCount,
                TotalInProgress = inProgressCount,
                TotalLocked = lockedCount,
                PassRatePercent = passRate
            };

            return View(vm);
        }
    }
}
