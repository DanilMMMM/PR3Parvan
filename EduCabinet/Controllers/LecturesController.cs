using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduCabinet.Data;
using EduCabinet.Models;
using EduCabinet.Services;

namespace EduCabinet.Controllers
{
    [Authorize]
    public class LecturesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProgressionService _progressionService;
        private readonly IWebHostEnvironment _env;

        public LecturesController(AppDbContext context, IProgressionService progressionService, IWebHostEnvironment env)
        {
            _context = context;
            _progressionService = progressionService;
            _env = env;
        }

        // GET: /Lectures/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Topic)
                .ThenInclude(t => t.Discipline)
                .Include(l => l.ControlQuestions.OrderBy(cq => cq.OrderIndex))
                .Include(l => l.Tests)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // CRITICAL SERVER-SIDE CHECK FOR STUDENTS (Sequential Progression Lock)
            if (User.IsInRole(UserRoles.Student))
            {
                int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var (canAccess, reason) = await _progressionService.CheckLectureAccessAsync(studentId, id);

                if (!canAccess)
                {
                    // Server-side refusal of forbidden direct URL access
                    TempData["ErrorMessage"] = reason ?? "Доступ к этой лекции заблокирован! Пройдите предыдущие темы.";
                    return RedirectToAction("Details", "Disciplines", new { id = lecture.Topic.DisciplineId });
                }

                // Get student's best attempt for this lecture's test
                var test = lecture.Tests.FirstOrDefault();
                TestAttempt? bestAttempt = null;
                int remainingAttempts = 0;

                if (test != null)
                {
                    var attempts = await _context.TestAttempts
                        .Where(ta => ta.StudentId == studentId && ta.TestId == test.Id)
                        .OrderByDescending(ta => ta.CompletedAt)
                        .ToListAsync();

                    bestAttempt = attempts.OrderByDescending(ta => ta.ScorePercent).FirstOrDefault();
                    remainingAttempts = Math.Max(0, test.MaxAttempts - attempts.Count);
                }

                var progress = await _context.LectureProgresses
                    .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LectureId == id);

                var vm = new LectureDetailViewModel
                {
                    Lecture = lecture,
                    IsUnlocked = true,
                    IsCompleted = progress?.IsCompleted ?? false,
                    Test = test,
                    BestAttempt = bestAttempt,
                    RemainingAttempts = remainingAttempts,
                    HasControlQuestions = lecture.ControlQuestions.Any()
                };

                return View(vm);
            }

            // For Teachers, access is always allowed
            var teacherVm = new LectureDetailViewModel
            {
                Lecture = lecture,
                IsUnlocked = true,
                IsCompleted = true,
                Test = lecture.Tests.FirstOrDefault(),
                HasControlQuestions = lecture.ControlQuestions.Any()
            };

            return View(teacherVm);
        }

        // GET: /Lectures/Create?topicId=1
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Create(int topicId)
        {
            var topic = await _context.Topics
                .Include(t => t.Discipline)
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null) return NotFound();

            ViewBag.Topic = topic;
            return View(new Lecture { TopicId = topicId, OrderIndex = 1, MinPassingScore = 70 });
        }

        // POST: /Lectures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Create(Lecture lecture, IFormFile? materialFile)
        {
            ModelState.Remove(nameof(lecture.Topic));
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (materialFile != null && materialFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "materials");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var safeFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(materialFile.FileName);
                    var filePath = Path.Combine(uploadsDir, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await materialFile.CopyToAsync(stream);
                    }

                    lecture.FilePath = "/uploads/materials/" + safeFileName;
                    lecture.OriginalFileName = materialFile.FileName;
                }

                _context.Lectures.Add(lecture);
                await _context.SaveChangesAsync();

                int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _context.AuditLogs.Add(new AuditLog
                {
                    TeacherId = teacherId,
                    Action = "CreateLecture",
                    Details = $"Создана лекция «{lecture.Title}» (Id={lecture.Id})",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Лекция «{lecture.Title}» успешно добавлена!";
                return RedirectToAction(nameof(Details), new { id = lecture.Id });
            }

            var topic = await _context.Topics.Include(t => t.Discipline).FirstOrDefaultAsync(t => t.Id == lecture.TopicId);
            ViewBag.Topic = topic;
            return View(lecture);
        }

        // GET: /Lectures/Edit/5
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Edit(int id)
        {
            var lecture = await _context.Lectures.Include(l => l.Topic).FirstOrDefaultAsync(l => l.Id == id);
            if (lecture == null) return NotFound();
            return View(lecture);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Edit(int id, Lecture model, IFormFile? materialFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove(nameof(model.Topic));
            if (ModelState.IsValid)
            {
                var lecture = await _context.Lectures.FindAsync(id);
                if (lecture == null) return NotFound();

                lecture.Title = model.Title;
                lecture.Description = model.Description;
                lecture.Content = model.Content;
                lecture.OrderIndex = model.OrderIndex;
                lecture.MinPassingScore = model.MinPassingScore;

                if (materialFile != null && materialFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "materials");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var safeFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(materialFile.FileName);
                    var filePath = Path.Combine(uploadsDir, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await materialFile.CopyToAsync(stream);
                    }

                    lecture.FilePath = "/uploads/materials/" + safeFileName;
                    lecture.OriginalFileName = materialFile.FileName;
                }

                await _context.SaveChangesAsync();

                int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _context.AuditLogs.Add(new AuditLog
                {
                    TeacherId = teacherId,
                    Action = "EditLecture",
                    Details = $"Преподаватель отредактировал лекцию «{lecture.Title}» (Id={lecture.Id})",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Лекция успешно обновлена!";
                return RedirectToAction(nameof(Details), new { id = lecture.Id });
            }

            return View(model);
        }

        // Download material file
        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null || string.IsNullOrEmpty(lecture.FilePath))
            {
                return NotFound("Файл материала не найден.");
            }

            if (User.IsInRole(UserRoles.Student))
            {
                int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var (canAccess, _) = await _progressionService.CheckLectureAccessAsync(studentId, id);
                if (!canAccess)
                {
                    return Forbid();
                }
            }

            var physicalPath = Path.Combine(_env.WebRootPath, lecture.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Файл отсутствует на сервере.");
            }

            var mimeType = "application/octet-stream";
            if (physicalPath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if (physicalPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                mimeType = "application/pdf";

            return PhysicalFile(physicalPath, mimeType, lecture.OriginalFileName ?? Path.GetFileName(physicalPath));
        }

        // POST: /Lectures/CheckControlQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckControlQuestion(int questionId, string studentAnswer)
        {
            var question = await _context.ControlQuestions.FindAsync(questionId);
            if (question == null) return Json(new { success = false, message = "Вопрос не найден" });

            bool isCorrect = string.Equals(question.CorrectAnswer.Trim(), studentAnswer?.Trim(), StringComparison.CurrentCultureIgnoreCase);

            return Json(new
            {
                success = true,
                isCorrect = isCorrect,
                correctAnswer = question.CorrectAnswer,
                message = isCorrect ? "Верно! Ответ правильный." : "Неверно. Попробуйте еще раз или сверьтесь с текстом лекции."
            });
        }
    }
}
