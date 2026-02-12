using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class AccountController : BaseController
    {
        private readonly STSStorage1Context _context;

        public AccountController(STSStorage1Context context)
        {
            _context = context;
        }

        public IActionResult LoginDb(bool timeout = false)
        {
            // Check if this is a redirect from another page (not first app load)
            var referer = Request.Headers["Referer"].ToString();
            var isRedirectFromApp = !string.IsNullOrEmpty(referer) &&
                                    !referer.Contains("/Account/LoginDb", StringComparison.OrdinalIgnoreCase) &&
                                    !referer.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase);

            // Only show timeout message if redirected from another page AND timeout flag is true
            if (timeout && isRedirectFromApp)
            {
                ViewBag.Message = "Your session has ended or timed out. Please log in again.";
            }

            // Always clear session when arriving at login page
            HttpContext.Session.Clear();

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Validate username is not empty
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.UserErrorMessage = "Username cannot be empty or null.";
                return View("LoginDb");
            }

            // Validate password is not empty
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.PasswordErrorMessage = "Password cannot be empty or null.";
                return View("LoginDb");
            }

            // Validate login credentials against the database
            var user = _context.InventoryUsers
                .SingleOrDefault(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ViewBag.UserErrorMessage = "Invalid username or password!";
                return View("LoginDb");
            }

            // Clear any old session data before setting new session
            HttpContext.Session.Clear();

            // Fetch the RoleName using the RoleId
            var role = _context.InventoryRole.SingleOrDefault(r => r.RoleId == user.Role_Id);

            if (role != null && !string.IsNullOrEmpty(role.RoleName))
            {
                HttpContext.Session.SetString("RoleName", role.RoleName);
            }
            else
            {
                HttpContext.Session.SetString("RoleName", string.Empty);
            }

            // Set session variables
            HttpContext.Session.SetInt32("MyID", user.MyID);
            HttpContext.Session.SetString("UserName", username);
            HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);
            HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));

            // Set temp data for login view only
            TempData["MyName"] = user.FirstName;
            TempData["MyId"] = user.MyID;

            // Successful login, redirect to home
            return RedirectToAction("STSHome", "Home");
        }

        public IActionResult RemoveSession()
        {
            // Clear session
            HttpContext.Session.Clear();
            return View("LoginDb");
        }

        public IActionResult Logout()
        {
            // Clear the session (log the user out)
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("LoginDb");
        }
    }
}

