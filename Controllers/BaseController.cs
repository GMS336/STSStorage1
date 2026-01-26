using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace STSStorage1.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var loginTimeStr = HttpContext.Session.GetString("LoginTime");
            if (!string.IsNullOrEmpty(loginTimeStr) && DateTime.TryParse(loginTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var loginTime))
            {
                // Set timeout to match your session timeout (e.g., 20 mins)
                var sessionTimeout = TimeSpan.FromMinutes(20);
                var sessionExpiry = loginTime.Add(sessionTimeout);
                ViewBag.SessionExpiry = sessionExpiry.ToUniversalTime().ToString("o");
            }
            else
            {
                ViewBag.SessionExpiry = null;
            }
        }
    }
}
