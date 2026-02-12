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

            // Check if the action has [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is AllowAnonymousAttribute);

            // Get session data
            var loginTimeStr = HttpContext.Session.GetString("LoginTime");
            var userName = HttpContext.Session.GetString("UserName");
            var fullName = HttpContext.Session.GetString("FullName");
            var roleName = HttpContext.Session.GetString("RoleName");

            // Check if user is actually logged in
            bool isLoggedIn = !string.IsNullOrEmpty(userName);

            if (allowAnonymous)
            {
                // For anonymous pages, check if there's an active session
                if (isLoggedIn && !string.IsNullOrEmpty(loginTimeStr) &&
                    DateTime.TryParse(loginTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var loginTime))
                {
                    // User is logged in, show timer
                    var sessionTimeout = TimeSpan.FromMinutes(1);
                    var sessionExpiry = loginTime.Add(sessionTimeout);
                    ViewBag.SessionExpiry = sessionExpiry.ToUniversalTime().ToString("o");
                    ViewBag.FullName = fullName;
                    ViewBag.RoleName = roleName;
                    ViewBag.LogInName = null; // Not showing login button
                }
                else
                {
                    // No session, show as public page
                    ViewBag.SessionExpiry = null;
                    ViewBag.FullName = null;
                    ViewBag.RoleName = null;
                    ViewBag.LogInName = "Log In!";
                }

                return;
            }

            // For authenticated pages, require session
            if (isLoggedIn && !string.IsNullOrEmpty(loginTimeStr) &&
                DateTime.TryParse(loginTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var authLoginTime))
            {
                var sessionTimeout = TimeSpan.FromMinutes(1);
                var sessionExpiry = authLoginTime.Add(sessionTimeout);
                ViewBag.SessionExpiry = sessionExpiry.ToUniversalTime().ToString("o");
                ViewBag.FullName = fullName;
                ViewBag.RoleName = roleName;
            }
            else
            {
                ViewBag.SessionExpiry = null;
            }
        }
    }
}