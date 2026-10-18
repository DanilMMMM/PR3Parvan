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
    [Authorize(Roles = UserRoles.Student)]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProgressionService _progressionService;

        public StudentController(AppDbContext context, IProgressionService progressionService)
        {
            _context = context;
            _progressionService = progressionService;
        }

        // GET: /Student/Index
        public async Task<IActionResult> Index()
        {
            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var student = await _context.Users
                .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null) return NotFound();

            var disciplines = await _context.Disciplines
                .Include(d => d.Topics)
                .ThenInclude(t => t.Lectures)
                .ThenInclude(l => l.Tests)
                .ToListAsync();

            var disciplineItems = new List<DisciplineProgressItem>();

            foreach (var d in disciplines)
            {
                int totalLecs = d.Topics.SelectMany(t => t.Lectures).Count();
                int progressPct = await _progressionService.CalculateDisciplineProgressPercentAsync(studentId, d.Id);
                var lectureStatuses = await _progressionService.GetLecturesWithStatusForStudentAsync(studentId, d.Id);
                var statusDict = lectureStatuses.ToDictionary(x => x.LectureId);

                var topicItems = new List<TopicItemViewModel>();
                foreach (var t in d.Topics.OrderBy(x => x.OrderIndex))
                {
                    var lecs = t.Lectures.OrderBy(l => l.OrderIndex).Select(l =>
                    {
                        statusDict.TryGetValue(l.Id, out var st);
                        return new LectureItemViewModel
                        {
                            LectureId = l.Id,
                            Title = l.Title,
                            OrderIndex = l.OrderIndex,
                            IsUnlocked = st?.IsUnlocked ?? false,
                            IsCompleted = st?.IsCompleted ?? false,
                            BestScorePercent = st?.BestScorePercent,
                            HasTest = l.Tests.Any()
                        };
                    }).ToList();

                    topicItems.Add(new TopicItemViewModel
                    {
                        TopicId = t.Id,
                        Title = t.Title,
                        Lectures = lecs
                    });
                }

                int completedCount = lectureStatuses.Count(l => l.IsCompleted);

                disciplineItems.Add(new DisciplineProgressItem
                {
                    DisciplineId = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    TotalLectures = totalLecs,
                    CompletedLectures = completedCount,
                    ProgressPercent = progressPct,
                    Topics = topicItems
                });
            }

            var recentAttempts = await _context.TestAttempts
                .Include(ta => ta.Test)
                .ThenInclude(t => t.Lecture)
                .Where(ta => ta.StudentId == studentId)
                .OrderByDescending(ta => ta.CompletedAt)
                .Take(10)
                .ToListAsync();

            var groupName = student.GroupMemberships.FirstOrDefault()?.Group?.Name ?? "Не назначена";

            var vm = new StudentDashboardViewModel
            {
                Student = student,
                GroupName = groupName,
                Disciplines = disciplineItems,
                RecentAttempts = recentAttempts
            };

            return View(vm);
        }
    }
}
