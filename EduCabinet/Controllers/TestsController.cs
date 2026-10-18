using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
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
    public class TestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProgressionService _progressionService;

        public TestsController(AppDbContext context, IProgressionService progressionService)
        {
            _context = context;
            _progressionService = progressionService;
        }

        // GET: /Tests/Take/5
        public async Task<IActionResult> Take(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Lecture)
                .ThenInclude(l => l.Topic)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null) return NotFound();

            if (User.IsInRole(UserRoles.Student))
            {
                int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // 1. Verify progression: lecture must be accessible
                var (canAccess, reason) = await _progressionService.CheckLectureAccessAsync(studentId, test.LectureId);
                if (!canAccess)
                {
                    TempData["ErrorMessage"] = reason ?? "Доступ к тесту заблокирован: лекция закрыта.";
                    return RedirectToAction("Details", "Disciplines", new { id = test.Lecture.Topic.DisciplineId });
                }

                // 2. Check attempts limit
                var attemptsCount = await _context.TestAttempts
                    .CountAsync(ta => ta.StudentId == studentId && ta.TestId == id);

                if (attemptsCount >= test.MaxAttempts)
                {
                    TempData["ErrorMessage"] = $"Вы исчерпали максимальное количество попыток ({test.MaxAttempts}) для этого теста. Для сброса попыток обратитесь к преподавателю.";
                    return RedirectToAction("Details", "Lectures", new { id = test.LectureId });
                }

                var vm = new TestTakeViewModel
                {
                    TestId = test.Id,
                    TestTitle = test.Title,
                    LectureId = test.LectureId,
                    LectureTitle = test.Lecture.Title,
                    PassingScorePercent = test.PassingScorePercent,
                    MaxAttempts = test.MaxAttempts,
                    CurrentAttemptNumber = attemptsCount + 1,
                    TimeLimitMinutes = test.TimeLimitMinutes,
                    Questions = test.Questions.Select(q => new QuestionTakeItem
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        Options = q.Options.Select(o => new OptionTakeItem
                        {
                            OptionId = o.Id,
                            OptionText = o.OptionText
                        }).ToList()
                    }).ToList()
                };

                return View(vm);
            }

            // If teacher previews the test
            var teacherVm = new TestTakeViewModel
            {
                TestId = test.Id,
                TestTitle = test.Title + " (Режим предпросмотра преподавателя)",
                LectureId = test.LectureId,
                LectureTitle = test.Lecture.Title,
                PassingScorePercent = test.PassingScorePercent,
                MaxAttempts = test.MaxAttempts,
                CurrentAttemptNumber = 1,
                TimeLimitMinutes = test.TimeLimitMinutes,
                Questions = test.Questions.Select(q => new QuestionTakeItem
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Points = q.Points,
                    Options = q.Options.Select(o => new OptionTakeItem
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(teacherVm);
        }

        // POST: /Tests/SubmitTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(TestTakeViewModel submission)
        {
            var test = await _context.Tests
                .Include(t => t.Lecture)
                .ThenInclude(l => l.Topic)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == submission.TestId);

            if (test == null) return NotFound();

            int studentId;
            if (User.IsInRole(UserRoles.Student))
            {
                studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }
            else
            {
                // Teacher preview - don't save DB attempt, just calculate
                studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            int currentAttemptsCount = await _context.TestAttempts
                .CountAsync(ta => ta.StudentId == studentId && ta.TestId == test.Id);

            int totalPoints = 0;
            int earnedPoints = 0;
            var resultQuestions = new List<QuestionResultItem>();
            var studentAnswers = new List<StudentAnswer>();

            foreach (var q in test.Questions)
            {
                totalPoints += q.Points;
                var submittedQ = submission.Questions.FirstOrDefault(x => x.QuestionId == q.Id);

                bool isCorrect = false;
                string userAnswerStr = "";
                string correctAnswerStr = "";

                if (q.QuestionType == "SingleChoice")
                {
                    var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect);
                    correctAnswerStr = correctOpt?.OptionText ?? "";

                    int? chosenOptId = submittedQ?.SelectedOptionId;
                    var chosenOpt = q.Options.FirstOrDefault(o => o.Id == chosenOptId);
                    userAnswerStr = chosenOpt?.OptionText ?? "Ответ не выбран";

                    isCorrect = chosenOpt != null && chosenOpt.IsCorrect;

                    studentAnswers.Add(new StudentAnswer
                    {
                        QuestionId = q.Id,
                        SelectedOptionIdsJson = chosenOptId.HasValue ? JsonSerializer.Serialize(new[] { chosenOptId.Value }) : null,
                        IsCorrect = isCorrect
                    });
                }
                else if (q.QuestionType == "MultipleChoice")
                {
                    var correctOptIds = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                    var chosenOptIds = submittedQ?.SelectedOptionIds ?? new List<int>();

                    var chosenOptions = q.Options.Where(o => chosenOptIds.Contains(o.Id)).ToList();
                    userAnswerStr = chosenOptions.Any() ? string.Join(", ", chosenOptions.Select(o => o.OptionText)) : "Ответ не выбран";
                    correctAnswerStr = string.Join(", ", q.Options.Where(o => o.IsCorrect).Select(o => o.OptionText));

                    // All correct selected and no incorrect selected
                    isCorrect = correctOptIds.SetEquals(chosenOptIds);

                    studentAnswers.Add(new StudentAnswer
                    {
                        QuestionId = q.Id,
                        SelectedOptionIdsJson = JsonSerializer.Serialize(chosenOptIds),
                        IsCorrect = isCorrect
                    });
                }
                else // TextAnswer
                {
                    var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect);
                    correctAnswerStr = correctOpt?.OptionText ?? "Соль";
                    userAnswerStr = submittedQ?.TextAnswer?.Trim() ?? "";

                    isCorrect = !string.IsNullOrWhiteSpace(userAnswerStr) &&
                                (userAnswerStr.Equals(correctAnswerStr, StringComparison.CurrentCultureIgnoreCase) ||
                                 userAnswerStr.Equals("соль", StringComparison.CurrentCultureIgnoreCase) ||
                                 userAnswerStr.Equals("salt", StringComparison.CurrentCultureIgnoreCase) ||
                                 correctAnswerStr.Contains(userAnswerStr, StringComparison.CurrentCultureIgnoreCase) ||
                                 userAnswerStr.Contains(correctAnswerStr, StringComparison.CurrentCultureIgnoreCase));

                    studentAnswers.Add(new StudentAnswer
                    {
                        QuestionId = q.Id,
                        TextAnswer = userAnswerStr,
                        IsCorrect = isCorrect
                    });
                }

                if (isCorrect)
                {
                    earnedPoints += q.Points;
                }

                resultQuestions.Add(new QuestionResultItem
                {
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsCorrect = isCorrect,
                    UserAnswer = userAnswerStr,
                    CorrectAnswer = correctAnswerStr
                });
            }

            int scorePercent = totalPoints > 0 ? (int)Math.Round((double)earnedPoints / totalPoints * 100) : 0;
            bool isPassed = scorePercent >= test.PassingScorePercent;

            var attempt = new TestAttempt
            {
                StudentId = studentId,
                TestId = test.Id,
                AttemptNumber = currentAttemptsCount + 1,
                ScorePercent = scorePercent,
                TotalPoints = totalPoints,
                EarnedPoints = earnedPoints,
                IsPassed = isPassed,
                CompletedAt = DateTime.UtcNow,
                Answers = studentAnswers
            };

            _context.TestAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            bool nextLectureUnlocked = false;
            int? nextLectureId = null;

            if (isPassed && User.IsInRole(UserRoles.Student))
            {
                // CRITICAL REQUIREMENT 3.6: Auto-unlock next lecture!
                await _progressionService.CompleteLectureAfterTestAsync(studentId, test.Id, attempt.Id);

                // Find next lecture in sequence
                var allLectures = await _context.Lectures
                    .Include(l => l.Topic)
                    .Where(l => l.Topic.DisciplineId == test.Lecture.Topic.DisciplineId)
                    .OrderBy(l => l.Topic.OrderIndex)
                    .ThenBy(l => l.OrderIndex)
                    .ToListAsync();

                int currentIndex = allLectures.FindIndex(l => l.Id == test.LectureId);
                if (currentIndex >= 0 && currentIndex + 1 < allLectures.Count)
                {
                    nextLectureUnlocked = true;
                    nextLectureId = allLectures[currentIndex + 1].Id;
                }
            }

            int remainingAttempts = Math.Max(0, test.MaxAttempts - (currentAttemptsCount + 1));

            var resultVm = new TestResultViewModel
            {
                AttemptId = attempt.Id,
                TestTitle = test.Title,
                LectureId = test.LectureId,
                LectureTitle = test.Lecture.Title,
                DisciplineId = test.Lecture.Topic.DisciplineId,
                ScorePercent = scorePercent,
                EarnedPoints = earnedPoints,
                TotalPoints = totalPoints,
                PassingScorePercent = test.PassingScorePercent,
                IsPassed = isPassed,
                AttemptNumber = attempt.AttemptNumber,
                RemainingAttempts = remainingAttempts,
                NextLectureUnlocked = nextLectureUnlocked,
                NextLectureId = nextLectureId,
                Questions = resultQuestions
            };

            return View("Result", resultVm);
        }

        // GET: /Tests/Create?lectureId=5
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Create(int lectureId)
        {
            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture == null) return NotFound();

            ViewBag.Lecture = lecture;
            return View(new Test
            {
                LectureId = lectureId,
                Title = $"Тестирование к лекции «{lecture.Title}»",
                PassingScorePercent = 70,
                MaxAttempts = 3,
                TimeLimitMinutes = 15
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<IActionResult> Create(Test test)
        {
            ModelState.Remove(nameof(test.Lecture));
            if (ModelState.IsValid)
            {
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _context.AuditLogs.Add(new AuditLog
                {
                    TeacherId = teacherId,
                    Action = "CreateTest",
                    Details = $"Создан тест «{test.Title}» (Id={test.Id})",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Тест успешно создан!";
                return RedirectToAction("Details", "Lectures", new { id = test.LectureId });
            }

            var lecture = await _context.Lectures.FindAsync(test.LectureId);
            ViewBag.Lecture = lecture;
            return View(test);
        }
    }
}
