using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;

namespace STSStorage1.Middleware
{
    public class SessionTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Allow requests to specific pages without a session
            if (context.Request.Path.StartsWithSegments("/Account/LoginDb", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/InvRegister/RegCreate", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/InvRegister/ForgotPassword", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Home/STSHome", StringComparison.OrdinalIgnoreCase)) // Exception for STSHome
            {
                await _next(context);
                return;
            }

            // Redirect to login page if the session is invalid
            if (context.Session.GetString("UserName") == null)
            {
                // Check if the user has already seen the welcome page (using session or cookie)
                if (context.Session.GetString("WelcomeShown") == null)
                {
                    // First time ever: show welcome page and set flag
                    context.Session.SetString("WelcomeShown", "true");
                    context.Response.Redirect("/Home/STSHome");
                    return;
                }
                else
                {
                    // Not the first time: show login page (maybe session expired)
                    context.Response.Redirect("/Account/LoginDb?timeout=true");
                    return;
                }
            }

            // Allow request to proceed if session is valid
            await _next(context);
        }
    }
}