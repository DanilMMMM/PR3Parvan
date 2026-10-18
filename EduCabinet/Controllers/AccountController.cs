using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduCabinet.Data;
using EduCabinet.Models;

namespace EduCabinet.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким логином не найден.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Неверный пароль.");
                return View(model);
            }

            // Create Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == UserRoles.Teacher)
            {
                return RedirectToAction("Index", "Teacher");
            }

            return RedirectToAction("Index", "Student");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var groups = await _context.StudentGroups.ToListAsync();
            ViewBag.Groups = new SelectList(groups, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var groups = await _context.StudentGroups.ToListAsync();
                ViewBag.Groups = new SelectList(groups, "Id", "Name");
                return View(model);
            }

            var existingUser = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());

            if (existingUser)
            {
                ModelState.AddModelError("Username", "Пользователь с таким логином уже существует.");
                var groups = await _context.StudentGroups.ToListAsync();
                ViewBag.Groups = new SelectList(groups, "Id", "Name");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username.Trim(),
                FullName = model.FullName.Trim(),
                Role = model.Role == UserRoles.Teacher ? UserRoles.Teacher : UserRoles.Student,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role == UserRoles.Student && model.GroupId.HasValue)
            {
                _context.GroupMemberships.Add(new GroupMembership
                {
                    StudentId = user.Id,
                    GroupId = model.GroupId.Value
                });
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Регистрация успешно выполнена! Теперь вы можете войти в систему.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
