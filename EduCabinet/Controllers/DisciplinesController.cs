using System;
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
    [Authorize]
    public class DisciplinesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProgressionService _progressionService;

        public DisciplinesController(AppDbContext context, IProgressionService progressionService)
        {
            _context = context;
            _progressionService = progressionService;
        }

        // GET: /Disciplines
        public async Task<IActionResult> Index()
        {
            var disciplines = await _context.Disciplines
                .Include(d => d.CreatedByTeacher)
                .Include(d => d.Topics)
                .ThenInclude(t => t.Lectures)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            if (User.IsInRole(UserRoles.Student))
            {
                int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                ViewBag.StudentId = studentId;

                var progressDict = new System.Collections.Generic.Dictionary<int, int>();
                foreach (var d in disciplines)
                {
                    progressDict[d.Id] = await _progressionService.CalculateDisciplineProgressPercentAsync(studentId, d.Id);
                }
                ViewBag.ProgressDict = progressDict;
            }

            return View(disciplines);
        }

        // GET: /Disciplines/Details/5
        public async Task<IActionResult> Details(int id, string? error = null)
        {
            var discipline = await _context.Disciplines
                .Include(d => d.CreatedByTeacher)
                .Include(d => d.Topics)
                .ThenInclude(t => t.Lectures)
                .ThenInclude(l => l.Tests)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discipline == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.ErrorMessage = error;
            }

            if (User.IsInRole(UserRoles.Student))
            {
                int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var lecturesStatus = await _progressionService.GetLecturesWithStatusForStudentAsync(studentId, id);
                ViewBag.LecturesStatus = lecturesStatus.ToDictionary(x => x.LectureId);
                ViewBag.ProgressPercent = await _progressionService.CalculateDisciplineProgressPercentAsync(studentId, id);
            }

            return View(discipline);
        }

        // GET: /Disciplines/Create
        [Authorize(Roles = UserRoles.Teacher)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Disciplines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Create(Discipline discipline)
        {
            if (ModelState.IsValid)
            {
                int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                discipline.CreatedByTeacherId = teacherId;
                discipline.CreatedAt = DateTime.UtcNow;

                _context.Disciplines.Add(discipline);
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    TeacherId = teacherId,
                    Action = "CreateDiscipline",
                    Details = $"Создана новая дисциплина «{discipline.Title}» (Id={discipline.Id})",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Дисциплина «{discipline.Title}» успешно создана!";
                return RedirectToAction(nameof(Details), new { id = discipline.Id });
            }

            return View(discipline);
        }

        // GET: /Disciplines/Edit/5
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Edit(int id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);
            if (discipline == null) return NotFound();
            return View(discipline);
        }

        // POST: /Disciplines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Edit(int id, Discipline model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var discipline = await _context.Disciplines.FindAsync(id);
                if (discipline == null) return NotFound();

                discipline.Title = model.Title;
                discipline.Description = model.Description;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Дисциплина успешно обновлена.";
                return RedirectToAction(nameof(Details), new { id = discipline.Id });
            }
            return View(model);
        }

        // POST: /Disciplines/AddTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> AddTopic(int disciplineId, string title, int orderIndex)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Название темы не может быть пустым.";
                return RedirectToAction(nameof(Details), new { id = disciplineId });
            }

            var topic = new Topic
            {
                DisciplineId = disciplineId,
                Title = title.Trim(),
                OrderIndex = orderIndex <= 0 ? 1 : orderIndex
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Тема «{topic.Title}» успешно добавлена.";
            return RedirectToAction(nameof(Details), new { id = disciplineId });
        }
    }
}
