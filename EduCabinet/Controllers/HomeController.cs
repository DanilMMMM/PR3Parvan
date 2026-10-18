using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EduCabinet.Models;

namespace EduCabinet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(UserRoles.Teacher))
                {
                    return RedirectToAction("Index", "Teacher");
                }
                if (User.IsInRole(UserRoles.Student))
                {
                    return RedirectToAction("Index", "Student");
                }
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
