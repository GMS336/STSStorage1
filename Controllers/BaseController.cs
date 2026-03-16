using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace STSStorage1.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Get action name FIRST
            var actionName = context.ActionDescriptor.RouteValues["action"];

            // Check if this is a logout/session clearing action
            var isLogoutAction = actionName == "Logout" || actionName == "RemoveSession" || actionName == "LoginDb";

            // IMMEDIATELY clear session for logout actions BEFORE any other logic
            if (isLogoutAction)
            {
                HttpContext.Session.Clear();
            }

            // Check if the action has [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is AllowAnonymousAttribute);

            // Get session data (will be empty for logout actions now)
            var loginTimeStr = HttpContext.Session.GetString("LoginTime");
            var userName = HttpContext.Session.GetString("UserName");
            var fullName = HttpContext.Session.GetString("FullName");
            var roleName = HttpContext.Session.GetString("RoleName");
            var phoneNum = HttpContext.Session.GetString("PhoneNum");

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"BaseController.OnActionExecuting - Action: {actionName}, AllowAnonymous: {allowAnonymous}, UserName: {userName}");

            // Check if user is actually logged in
            bool isLoggedIn = !string.IsNullOrEmpty(userName);

            if (allowAnonymous)
            {
                // For anonymous pages, check if there's an active session
                if (isLoggedIn && !string.IsNullOrEmpty(loginTimeStr) &&
                    DateTime.TryParse(loginTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var loginTime))
                {
                    // User is logged in, show timer
                    var sessionTimeout = TimeSpan.FromMinutes(10);
                    var sessionExpiry = loginTime.Add(sessionTimeout);
                    ViewBag.SessionExpiry = sessionExpiry.ToUniversalTime().ToString("o");
                    ViewBag.FullName = fullName;
                    ViewBag.RoleName = roleName;
                    ViewBag.PhoneNum = phoneNum;
                    ViewBag.MyID = HttpContext.Session.GetInt32("MyID");
                    ViewBag.LogInName = userName; // Set to username when logged in
                }
                else
                {
                    // No session, show as public page
                    ViewBag.SessionExpiry = null;
                    ViewBag.FullName = null;
                    ViewBag.RoleName = null;
                    ViewBag.PhoneNum = null;
                    ViewBag.MyID = null;
                    ViewBag.LogInName = "Log In!"; // Set to "Log In!" when not logged in
                }

                return;
            }

            // For authenticated pages, require session
            if (isLoggedIn && !string.IsNullOrEmpty(loginTimeStr) &&
                DateTime.TryParse(loginTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var authLoginTime))
            {
                var sessionTimeout = TimeSpan.FromMinutes(10);
                var sessionExpiry = authLoginTime.Add(sessionTimeout);
                ViewBag.SessionExpiry = sessionExpiry.ToUniversalTime().ToString("o");
                ViewBag.FullName = fullName;
                ViewBag.RoleName = roleName;
                ViewBag.PhoneNum = phoneNum;
                ViewBag.MyID = HttpContext.Session.GetInt32("MyID");
                ViewBag.LogInName = userName; // Set to username when logged in
            }
            else
            {
                ViewBag.SessionExpiry = null;
                ViewBag.FullName = null;
                ViewBag.RoleName = null;
                ViewBag.PhoneNum = null;
                ViewBag.MyID = null;
                ViewBag.LogInName = "Log In!"; // Set to "Log In!" when not logged in
            }
        }
    }
}