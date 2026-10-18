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
    }
}
