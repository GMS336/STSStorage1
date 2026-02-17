# Session Management System - Complete Documentation
**Project**: STSStorage1  
**Last Updated**: February 2026  
**Version**: 1.0

---

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Components](#components)
4. [Features](#features)
5. [Configuration](#configuration)
6. [Implementation Details](#implementation-details)
7. [Testing Guide](#testing-guide)
8. [Troubleshooting](#troubleshooting)
9. [Code Reference](#code-reference)

---

## Overview

This session management system provides secure, user-friendly authentication and session handling for the STSStorage1 application. It includes automatic session timeout, activity tracking, and proper handling of public vs. protected pages.

### Key Capabilities
- ✅ 1-minute session timeout with countdown timer
- ✅ Automatic session reset on user activity (every 30 seconds)
- ✅ One-click logout with immediate session clearing
- ✅ Public pages accessible without authentication
- ✅ Protected pages with automatic redirect
- ✅ Role-based access control
- ✅ Clean separation of concerns

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Browser                        │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ _Layout    │  │  main.js     │  │  Session Timer   │   │
│  │ (UI View)  │  │ (Activity)   │  │  (Countdown)     │   │
│  └────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼ HTTP Requests
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Pipeline                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SessionTimeoutMiddleware                            │  │
│  │  - Route validation                                  │  │
│  │  - Session checking                                  │  │
│  │  - Redirect on timeout                               │  │
│  └───────────────────────────────────���──────────────────┘  │
│                            │                                 │
│                            ▼                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  BaseController                                       │  │
│  │  - Early session clearing for logout                 │  │
│  │  - ViewBag management                                │  │
│  │  - [AllowAnonymous] detection                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                 │
│                            ▼                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  AccountController                                    │  │
│  │  - Login/Logout actions                              │  │
│  │  - ResetSessionTimer API                             │  │
│  │  - Session variable management                       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     Session Storage                          │
│  - UserName, FullName, RoleName                             │
│  - LoginTime (UTC timestamp)                                │
│  - MyID (User identifier)                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Components

### 1. **Program.cs** - Application Configuration
```csharp
// Session timeout: 1 minute
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Default route to login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginDb}/{id?}");
```

**Purpose**: Configures session settings and default routing.

---

### 2. **SessionTimeoutMiddleware.cs** - Route Protection

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Allow requests to specific pages without a session
    if (context.Request.Path.StartsWithSegments("/Account/LoginDb") ||
        context.Request.Path.StartsWithSegments("/Account/Login") ||
        context.Request.Path.StartsWithSegments("/Home/STSHome")) // Welcome page
    {
        await _next(context);
        return;
    }

    // Redirect to login page if the session is invalid
    if (context.Session.GetString("UserName") == null)
    {
        var isRootPath = context.Request.Path == "/" || 
                         string.IsNullOrEmpty(context.Request.Path.Value);
        
        if (isRootPath)
        {
            context.Response.Redirect("/Account/LoginDb");
        }
        else
        {
            context.Response.Redirect("/Account/LoginDb?timeout=true");
        }
        return;
    }

    await _next(context);
}
```

**Purpose**: 
- Protects routes requiring authentication
- Allows public pages (LoginDb, Welcome)
- Redirects unauthorized users with appropriate timeout flag

---

### 3. **BaseController.cs** - ViewBag Management

```csharp
public override void OnActionExecuting(ActionExecutingContext context)
{
    base.OnActionExecuting(context);

    // Get action name FIRST
    var actionName = context.ActionDescriptor.RouteValues["action"];
    
    // Check if this is a logout/session clearing action
    var isLogoutAction = actionName == "Logout" || 
                         actionName == "RemoveSession" || 
                         actionName == "LoginDb";

    // IMMEDIATELY clear session for logout actions
    if (isLogoutAction)
    {
        HttpContext.Session.Clear();
    }

    // Check if the action has [AllowAnonymous] attribute
    var allowAnonymous = context.ActionDescriptor.EndpointMetadata
        .Any(m => m is AllowAnonymousAttribute);

    // Get session data (will be empty for logout actions)
    var userName = HttpContext.Session.GetString("UserName");
    var fullName = HttpContext.Session.GetString("FullName");
    var roleName = HttpContext.Session.GetString("RoleName");
    var loginTimeStr = HttpContext.Session.GetString("LoginTime");

    // Set ViewBag values based on session state
    if (allowAnonymous)
    {
        if (!string.IsNullOrEmpty(userName))
        {
            // Logged in user visiting public page
            ViewBag.LogInName = userName;
            ViewBag.FullName = fullName;
            ViewBag.RoleName = roleName;
            // Calculate session expiry for timer
        }
        else
        {
            // Anonymous user
            ViewBag.LogInName = "Log In!";
            ViewBag.FullName = null;
            ViewBag.RoleName = null;
        }
    }
}
```

**Purpose**:
- Clears session BEFORE reading it for logout actions (critical for one-click logout)
- Detects `[AllowAnonymous]` attribute
- Sets ViewBag values for layout rendering
- Calculates session expiry for timer display

---

### 4. **AccountController.cs** - Authentication Logic

#### Login Action
```csharp
[HttpPost]
public IActionResult Login(string username, string password)
{
    // Validate credentials
    var user = _context.InventoryUsers
        .SingleOrDefault(u => u.UserName == username && u.Password == password);

    if (user == null)
    {
        ViewBag.UserErrorMessage = "Invalid username or password!";
        return View("LoginDb");
    }

    // Clear any old session
    HttpContext.Session.Clear();

    // Set session variables
    HttpContext.Session.SetInt32("MyID", user.MyID);
    HttpContext.Session.SetString("UserName", username);
    HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);
    HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));
    HttpContext.Session.SetString("RoleName", roleName);

    return RedirectToAction("STSHome", "Home");
}
```

#### Logout Action
```csharp
[AllowAnonymous]
public IActionResult Logout()
{
    // Session already cleared in BaseController.OnActionExecuting
    return View("LoginDb");
}
```

#### Reset Session Timer (Activity Tracking API)
```csharp
[HttpPost]
public IActionResult ResetSessionTimer()
{
    var userName = HttpContext.Session.GetString("UserName");
    
    if (string.IsNullOrEmpty(userName))
    {
        return Json(new { success = false, message = "No active session" });
    }

    // Update LoginTime to reset timer
    var newLoginTime = DateTime.UtcNow;
    HttpContext.Session.SetString("LoginTime", newLoginTime.ToString("o"));

    // Calculate new expiry
    var sessionTimeout = TimeSpan.FromMinutes(1);
    var newExpiry = newLoginTime.Add(sessionTimeout);

    return Json(new 
    { 
        success = true, 
        newExpiry = newExpiry.ToString("o"),
        message = "Session timer reset"
    });
}
```

**Purpose**:
- Handles user authentication
- Manages session creation and destruction
- Provides API endpoint for activity tracking

---

### 5. **main.js** - Client-Side Timer & Activity Tracking

#### Global Activity Tracking
```javascript
(function () {
    var lastActivityTime = 0;
    var activityThrottleMs = 30000; // 30 seconds
    
    function handleActivity() {
        var now = Date.now();
        
        if (now - lastActivityTime > activityThrottleMs) {
            fetch('/Account/ResetSessionTimer', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success && data.newExpiry) {
                    lastActivityTime = Date.now(); // Reset throttle
                    window.sessionExpiry = data.newExpiry;
                    window.dispatchEvent(new CustomEvent('sessionReset', 
                        { detail: { newExpiry: data.newExpiry } }));
                }
            });
        }
    }
    
    // Listen on entire document
    document.addEventListener('click', handleActivity);
    document.addEventListener('keypress', handleActivity);
    document.addEventListener('scroll', handleActivity);
    document.addEventListener('mousemove', handleActivity);
})();
```

#### Timer Display
```javascript
(function () {
    if (typeof window.sessionExpiry !== 'undefined' && window.sessionExpiry) {
        var expiry = new Date(window.sessionExpiry);
        var timerDiv = document.getElementById('session-timer');
        
        // Listen for session reset events
        window.addEventListener('sessionReset', function(event) {
            expiry = new Date(event.detail.newExpiry);
        });
        
        function updateTimer() {
            var now = new Date();
            var diff = expiry - now;
            
            if (diff <= 0) {
                timerDiv.textContent = "Session expired";
                window.location.href = '/Account/LoginDb?timeout=true';
            } else {
                var mins = Math.floor(diff / 60000);
                var secs = Math.floor((diff % 60000) / 1000);
                timerDiv.textContent = "Session expires in " + mins + "m " + secs + "s";
            }
        }
        
        updateTimer();
        setInterval(updateTimer, 1000);
    }
})();
```

**Purpose**:
- Global activity tracking (works on all pages)
- Countdown timer display
- Automatic redirect on expiry
- Communication between activity tracking and timer display via custom events

---

### 6. **_Layout.cshtml** - UI Rendering

```razor
@{
    var isLoginPage = Context.Request.Path.StartsWithSegments("/Account/LoginDb") ||
                      Context.Request.Path.StartsWithSegments("/Account/Login") ||
                      ViewBag.LogInName == "Log In!";
}

<!-- Header controls -->
<div class="header-bar">
    @if (isLoginPage)
    {
        <a asp-controller="Account" asp-action="LoginDb">@ViewBag.LogInName</a>
    }
    else
    {
        <a asp-controller="invUsers" asp-action="ProfileEdit" 
           asp-route-id="@ViewBag.MyID">Edit Profile</a>
        <a asp-controller="Account" asp-action="Logout">Log Out</a>
    }
</div>

<!-- User info (only when logged in) -->
@if (!isLoginPage)
{
    <span>Welcome: @ViewBag.FullName<br />@ViewBag.RoleName</span>
}

<!-- Session timer (only when logged in) -->
@if (ViewBag.SessionExpiry != null)
{
    <div id="session-timer"></div>
    <script>
        window.sessionExpiry = '@ViewBag.SessionExpiry';
    </script>
}
```

**Purpose**:
- Conditional rendering based on login state
- Session timer initialization
- User information display

---

## Features

### 1. Automatic Session Timeout
- **Duration**: 1 minute of inactivity
- **Display**: Countdown timer shows remaining time
- **Behavior**: Redirects to login with timeout message

### 2. Activity Tracking
- **Triggers**: Click, keypress, scroll, mousemove
- **Frequency**: Resets session every 30 seconds of activity
- **Scope**: Works on ALL pages globally
- **Performance**: Throttled to prevent excessive server requests

### 3. One-Click Logout
- **Mechanism**: Early session clearing in BaseController
- **Result**: Immediate UI update without second click
- **Process**: 
  1. User clicks "Log Out"
  2. BaseController detects logout action
  3. Session cleared BEFORE reading ViewBag values
  4. View rendered with logged-out state

### 4. Public vs Protected Pages
- **Public**: Welcome, Login, Register (accessible without authentication)
- **Protected**: All other pages require valid session
- **Middleware**: Automatically redirects unauthenticated users

### 5. Role-Based Access
- **Roles**: Admin, Manager, User
- **Storage**: RoleName stored in session
- **Usage**: Controllers check role for authorization

---

## Configuration

### Session Timeout Settings
**File**: `Program.cs`

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1); // Change duration here
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### Activity Tracking Throttle
**File**: `wwwroot/js/main.js`

```javascript
var activityThrottleMs = 30000; // Change to 15000 for 15 seconds, etc.
```

### Public Pages Configuration
**File**: `Middleware/SessionTimeoutMiddleware.cs`

```csharp
// Add more public pages here
if (context.Request.Path.StartsWithSegments("/Account/LoginDb") ||
    context.Request.Path.StartsWithSegments("/Home/STSHome") ||
    context.Request.Path.StartsWithSegments("/YourNewPublicPage"))
{
    await _next(context);
    return;
}
```

---

## Implementation Details

### Session Variables Stored
| Variable | Type | Purpose |
|----------|------|---------|
| `UserName` | string | Unique username for authentication |
| `FullName` | string | Display name (FirstName + LastName) |
| `RoleName` | string | User's role (Admin, Manager, User) |
| `LoginTime` | string | ISO 8601 UTC timestamp of login |
| `MyID` | int | Database user ID |

### Execution Flow

#### Login Flow
```
1. User submits credentials
2. AccountController.Login validates
3. Session.Clear() removes old data
4. New session variables set
5. LoginTime = DateTime.UtcNow
6. Redirect to Home
7. BaseController.OnActionExecuting sets ViewBag
8. Timer initialized with expiry time
9. Activity tracking starts
```

#### Activity Reset Flow
```
1. User performs action (click, type, etc.)
2. JavaScript detects activity
3. Check: Has 30 seconds passed since last reset?
4. YES: Send POST to /Account/ResetSessionTimer
5. Server updates LoginTime to now
6. Server returns new expiry time
7. JavaScript updates window.sessionExpiry
8. JavaScript dispatches 'sessionReset' event
9. Timer display updates with new expiry
10. lastActivityTime reset to allow next reset
```

#### Logout Flow
```
1. User clicks "Log Out"
2. Request goes to AccountController.Logout
3. BaseController.OnActionExecuting intercepts
4. Detects action name = "Logout"
5. Session.Clear() IMMEDIATELY (before reading)
6. Continues to check session variables (all null)
7. Sets ViewBag.LogInName = "Log In!"
8. Returns to Logout action
9. Logout returns View("LoginDb")
10. Layout renders with logged-out UI
```

#### Timeout Flow
```
1. User inactive for 1 minute
2. Timer counts down to zero
3. JavaScript detects diff <= 0
4. Changes timer text to "Session expired"
5. Redirects to /Account/LoginDb?timeout=true
6. Controller shows timeout message
7. Session cleared
```

---

## Testing Guide

### Test Case 1: Login and Timer Display
1. Navigate to app (should show login page)
2. Enter valid credentials
3. Click "Login"
4. **Expected**: 
   - Redirected to home page
   - Timer shows "Session expires in 1m 0s"
   - User name displayed in header
   - "Log Out" button visible

### Test Case 2: Activity Tracking
1. Log in
2. Wait 40 seconds (timer shows ~0m 20s remaining)
3. Click any button or move mouse
4. Wait 35 more seconds
5. **Expected**: 
   - After first activity, console shows "Session timer reset"
   - Timer resets to 1m 0s
   - After 35 seconds, timer shows ~0m 25s
   - System doesn't log out

### Test Case 3: Activity Throttling
1. Log in
2. Wait 40 seconds
3. Click button (should reset timer)
4. Immediately click button again
5. **Expected**: 
   - First click resets timer
   - Second click shows "Activity throttled" in console
   - Timer doesn't reset again until 30 seconds pass

### Test Case 4: Session Timeout
1. Log in
2. Don't interact with page for 1 full minute
3. **Expected**:
   - Timer counts down to zero
   - Shows "Session expired"
   - Redirects to login page
   - Message: "Your session has ended or timed out. Please log in again."

### Test Case 5: One-Click Logout
1. Log in
2. Click "Log Out" button once
3. **Expected**:
   - Immediately shows login page
   - "Log In!" button visible
   - No user info in header
   - No timer visible
   - Welcome button works without login

### Test Case 6: Public Page Access
1. Without logging in, click "Welcome / Home"
2. **Expected**:
   - Welcome page loads
   - "Log In!" button visible
   - No session timer
   - No user info

### Test Case 7: Protected Page Access
1. Without logging in, try to access `/InvAddNew/Create`
2. **Expected**:
   - Redirected to login page
   - Message: "Your session has ended or timed out. Please log in again."

### Test Case 8: App Startup
1. Close all browser tabs
2. Stop and restart the app
3. Navigate to app URL
4. **Expected**:
   - Shows login page
   - NO timeout message (this was a bug, now fixed)
   - "Log In!" button visible

---

## Troubleshooting

### Issue: Logout requires two clicks

**Symptoms**: 
- First click on "Log Out" doesn't clear session
- User info still visible in header
- Second click required to fully log out

**Cause**: BaseController reads session BEFORE Logout action clears it

**Solution**: 
```csharp
// In BaseController.OnActionExecuting
var actionName = context.ActionDescriptor.RouteValues["action"];
var isLogoutAction = actionName == "Logout" || actionName == "RemoveSession" || actionName == "LoginDb";

if (isLogoutAction)
{
    HttpContext.Session.Clear(); // Clear FIRST, before reading
}
```

---

### Issue: Activity tracking only resets once

**Symptoms**:
- Timer resets on first activity
- Subsequent activities don't reset timer
- User gets logged out despite being active

**Cause**: `lastActivityTime` not reset after successful session reset

**Solution**:
```javascript
.then(data => {
    if (data.success && data.newExpiry) {
        lastActivityTime = Date.now(); // CRITICAL: Reset throttle
        window.sessionExpiry = data.newExpiry;
    }
});
```

---

### Issue: Activity tracking doesn't work on all pages

**Symptoms**:
- Timer resets on home page
- Timer doesn't reset on other pages
- Menu clicks don't trigger reset

**Cause**: Event listeners only attached when `window.sessionExpiry` exists

**Solution**: Separate activity tracking from timer display
```javascript
// Global activity tracking (outside timer check)
(function () {
    // Activity tracking code here
})();

// Timer display (inside sessionExpiry check)
(function () {
    if (typeof window.sessionExpiry !== 'undefined') {
        // Timer display code here
    }
})();
```

---

### Issue: Timeout message shows on app startup

**Symptoms**:
- Fresh app start shows "Session expired" message
- Happens when starting from root path `/`

**Cause**: Middleware redirects root path with `timeout=true`

**Solution**:
```csharp
// In SessionTimeoutMiddleware
var isRootPath = context.Request.Path == "/" || string.IsNullOrEmpty(context.Request.Path.Value);

if (isRootPath)
{
    context.Response.Redirect("/Account/LoginDb"); // No timeout param
}
else
{
    context.Response.Redirect("/Account/LoginDb?timeout=true");
}
```

---

### Issue: Session variables not persisting

**Symptoms**:
- User logs in successfully
- Session variables null on next page
- User immediately logged out

**Cause**: 
1. Session middleware not registered in Program.cs
2. Session used before `app.UseSession()` called

**Solution**:
```csharp
// In Program.cs - ORDER MATTERS
app.UseSession(); // Must be before UseEndpoints
app.UseMiddleware<SessionTimeoutMiddleware>(); // After UseSession
app.MapControllers();
```

---

### Issue: Timer doesn't update after activity reset

**Symptoms**:
- Activity tracking works (console shows reset)
- Timer display doesn't refresh
- Timer continues counting down

**Cause**: No communication between activity tracking and timer display

**Solution**: Use custom events
```javascript
// In activity tracking
window.dispatchEvent(new CustomEvent('sessionReset', 
    { detail: { newExpiry: data.newExpiry } }));

// In timer display
window.addEventListener('sessionReset', function(event) {
    expiry = new Date(event.detail.newExpiry);
});
```

---

## Code Reference

### Quick Reference: Key Files

| File | Lines of Code | Primary Function |
|------|---------------|------------------|
| `Program.cs` | ~100 | App configuration, session setup |
| `SessionTimeoutMiddleware.cs` | ~55 | Route protection |
| `BaseController.cs` | ~90 | ViewBag management, session clearing |
| `AccountController.cs` | ~135 | Authentication, session API |
| `main.js` | ~95 | Timer display, activity tracking |
| `_Layout.cshtml` | ~150 | UI rendering |

### Session Variable Access Pattern

**Setting Variables:**
```csharp
HttpContext.Session.SetString("UserName", username);
HttpContext.Session.SetInt32("MyID", userId);
HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));
```

**Reading Variables:**
```csharp
var userName = HttpContext.Session.GetString("UserName");
var userId = HttpContext.Session.GetInt32("MyID");
var loginTimeStr = HttpContext.Session.GetString("LoginTime");
```

**Checking If Session Exists:**
```csharp
bool isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserName"));
```

**Clearing Session:**
```csharp
HttpContext.Session.Clear();
```

---

### ViewBag Values Set by BaseController

| ViewBag Property | Type | Purpose |
|------------------|------|---------|
| `ViewBag.LogInName` | string | "Log In!" or username |
| `ViewBag.FullName` | string | User's full name |
| `ViewBag.RoleName` | string | User's role |
| `ViewBag.MyID` | int? | User database ID |
| `ViewBag.SessionExpiry` | string | ISO 8601 expiry timestamp |

**Usage in Views:**
```razor
@if (ViewBag.LogInName != "Log In!")
{
    <span>Welcome: @ViewBag.FullName</span>
}

@if (ViewBag.SessionExpiry != null)
{
    <div id="session-timer"></div>
    <script>window.sessionExpiry = '@ViewBag.SessionExpiry';</script>
}
```

---

## Performance Considerations

### Server-Side
- **Session Storage**: In-memory (fast, but lost on app restart)
- **Session Size**: ~500 bytes per user (minimal)
- **Database Queries**: 2 per login (user lookup, role lookup)
- **API Calls**: ~2 per minute per active user (activity tracking)

### Client-Side
- **Timer Update**: Every 1 second (UI only, no server call)
- **Activity Throttle**: 30 seconds minimum between server calls
- **Event Listeners**: 4 global listeners (click, keypress, scroll, mousemove)
- **Memory Usage**: <1 KB per page

### Scalability
- **Concurrent Users**: Limited by session storage (recommend Redis for >1000 users)
- **Activity Tracking Load**: Max 2 requests/minute/user = 120 req/hour/user
- **For 100 concurrent users**: ~200 req/minute for activity tracking

---

## Security Considerations

### Current Implementation
✅ HttpOnly cookies (prevents XSS access)  
✅ Session timeout (limits exposure window)  
✅ Server-side validation (all auth checks on server)  
✅ No sensitive data in ViewBag (only display names)  
✅ Automatic redirect on session expiry  

### Recommendations for Production
⚠️ Use HTTPS only (encrypt session cookies in transit)  
⚠️ Implement password hashing (currently stored in plain text)  
⚠️ Add CSRF protection for POST requests  
⚠️ Use Redis or SQL Server for session storage (survives app restarts)  
⚠️ Implement rate limiting on login attempts  
⚠️ Add logging for security events (failed logins, session hijacking attempts)  
⚠️ Consider implementing "Remember Me" with separate long-lived tokens  

---

## Future Enhancements

### Potential Improvements
1. **Session Warning**: Alert user at 30 seconds remaining
2. **"Keep Me Logged In"**: Extend timeout for trusted devices
3. **Auto-Save**: Save draft data before session expires
4. **Multiple Device Detection**: Notify user if logged in elsewhere
5. **Activity Logging**: Track user actions for audit trail
6. **Progressive Timeout**: Shorter timeout for sensitive pages
7. **Session Handoff**: Transfer session between devices

---

## Changelog

### Version 1.0 (February 2026)
- ✅ Initial implementation
- ✅ 1-minute session timeout
- ✅ Activity tracking with auto-reset
- ✅ One-click logout
- ✅ Public page support
- ✅ Role-based access control
- ✅ Countdown timer display
- ✅ Fixed: Logout requiring two clicks
- ✅ Fixed: Activity tracking only working once
- ✅ Fixed: Activity tracking only working on one page
- ✅ Fixed: Timeout message on app startup

---

## Support & Maintenance

### Common Maintenance Tasks

**Changing Session Timeout:**
1. Edit `Program.cs`, line ~25: `options.IdleTimeout = TimeSpan.FromMinutes(X);`
2. Edit `BaseController.cs`, line ~33 and ~58: `var sessionTimeout = TimeSpan.FromMinutes(X);`
3. Edit `AccountController.cs`, line ~107: `var sessionTimeout = TimeSpan.FromMinutes(X);`

**Adding New Public Page:**
1. Edit `Middleware/SessionTimeoutMiddleware.cs`
2. Add path to allowlist:
   ```csharp
   if (context.Request.Path.StartsWithSegments("/YourNewPage"))
   {
       await _next(context);
       return;
   }
   ```

**Changing Activity Tracking Frequency:**
1. Edit `wwwroot/js/main.js`, line ~11: `var activityThrottleMs = X;` (milliseconds)
2. Lower = more frequent resets, more server load
3. Higher = less frequent resets, user may timeout while active

---

## License & Credits

**Project**: STSStorage1  
**Organization**: Magna Powertrain - Engineering and Test Services  
**Session Management System**: Custom implementation  
**Framework**: ASP.NET Core 6.0  
**Database**: SQL Server with Entity Framework Core  

---

## Appendix: Complete Code Listing

### Program.cs (Session Configuration)
```csharp
// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Middleware order
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Before middleware
app.UseMiddleware<SessionTimeoutMiddleware>();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginDb}/{id?}");
```

---

**End of Documentation**

*For questions or issues, refer to the troubleshooting section or contact the development team.*