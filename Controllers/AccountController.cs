using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        public IActionResult LoginDb(bool timeout = false)
        {
            // Show timeout message if timeout parameter is true
            if (timeout)
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

        [HttpPost]
        public IActionResult ResetSessionTimer()
        {
            // Check if user is logged in
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                // No session, return failure
                return Json(new { success = false, message = "No active session" });
            }

            // Update LoginTime to current time (resets the session timer)
            var newLoginTime = DateTime.UtcNow;
            HttpContext.Session.SetString("LoginTime", newLoginTime.ToString("o"));

            // Calculate new expiry time (1 minute from now)
            var sessionTimeout = TimeSpan.FromMinutes(1);
            var newExpiry = newLoginTime.Add(sessionTimeout);

            return Json(new
            {
                success = true,
                newExpiry = newExpiry.ToString("o"),
                message = "Session timer reset"
            });
        }

        [AllowAnonymous]
        public IActionResult RemoveSession()
        {
            // Clear session and return to login view
            HttpContext.Session.Clear();
            return View("LoginDb");
        }

        [AllowAnonymous]
        public IActionResult Logout()
        {
            // Debug: Log session state BEFORE clearing
            var userNameBefore = HttpContext.Session.GetString("UserName");
            System.Diagnostics.Debug.WriteLine($"Logout called - UserName BEFORE clear: {userNameBefore}");

            // Clear the session (log the user out)
            HttpContext.Session.Clear();

            // Debug: Log session state AFTER clearing
            var userNameAfter = HttpContext.Session.GetString("UserName");
            System.Diagnostics.Debug.WriteLine($"Logout called - UserName AFTER clear: {userNameAfter}");

            // Return login view directly (no redirect) to ensure session is cleared
            return View("LoginDb");
        }
    }
}