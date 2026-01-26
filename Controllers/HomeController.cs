using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSStorage1.Models;
using System.Diagnostics;


namespace STSStorage1.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous] // <-- This allows anyone to view this action,
                         // even if the rest of the app requires auth
        public IActionResult STSHome()
        {
            return View();
        }

        //public IActionResult Welcome()
        //{
        //    return View();
        //}

        public IActionResult ProfileEdit(int? id)
        {
            // Retrieving value from session
            //var EmailLogin = HttpContext.Session.GetString("bolAuthenticate");
            //ViewBag.Message1 = EmailLogin ?? "Session is empty!";
            return View();
        }

        public IActionResult Privacy()
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
