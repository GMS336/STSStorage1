using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

using System.Diagnostics.CodeAnalysis;

namespace STSStorage1.Controllers

{
public class AccountController : BaseController
    {
    private readonly STSStorage1Context _context;
    private object? user;
    private object? pass;

    public AccountController(STSStorage1Context context)
    {
        _context = context;
    }

    public IActionResult LoginDb(bool timeout = false)
    {
        // Check if the timeout query parameter is true
        if (timeout)
        {
            ViewBag.Message = "Your session has ended or timed out. Please log in again.";
        }
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {

        //get error empty or nulll value validation before going to database.
        if (string.IsNullOrEmpty(username))
        {
            ViewBag.UserErrorMessage = "Username cannot be empty or null.";
            return View("LoginDb"); // Return the same view with the error message
        }
        // Validate login credentials against the database
        var user = _context.InventoryUsers.SingleOrDefault(u => u.UserName == username);
        if (user == null)
        {
            ViewBag.UserErrorMessage = "Invalid User Name!";
            return View("LoginDb"); // Return the same view with the error message
        }

        //get error empty or null value validation before going to database.
        if (string.IsNullOrEmpty(password))
        {
            ViewBag.PasswordErrorMessage = "Password cannot be empty or null.";
            return View("LoginDb"); // Return the same view with the error message
        }
        // Validate login credentials against the database
        var pass = _context.InventoryUsers.First(u => u.Password == password);
        if (pass == null)
        {
            ViewBag.PasswordErrorMessage = "Invalid Password!";
            return View("LoginDb"); // Return the same view with the error message
        }


        // if the login is successful, set the session variables
        // Fetch the RoleName using the RoleId from the Login table
        var role = _context.InventoryRole.SingleOrDefault(r => r.RoleId == user.Role_Id);

            if (role != null && !string.IsNullOrEmpty(role.RoleName))
            {
                HttpContext.Session.SetString("RoleName", role.RoleName);
            }
            else
            {
                HttpContext.Session.SetString("RoleName", string.Empty); // or handle as needed
            }

        // sets the session variables
        HttpContext.Session.SetInt32("MyID", user.MyID);
        HttpContext.Session.SetString("UserName", username);
        HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);  // concatenate first and last name
        HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));
        
        // sets a temp session for use in login view only.
        TempData["MyName"] = user.FirstName;
        TempData["MyId"] = user.MyID;

        // Successful login, redirect to a dashboard or home page
        return RedirectToAction("STSHome", "Home");

    }
    public IActionResult RemoveSession()
    {
        // Removing value from session
        HttpContext.Session.Clear();
        //HttpContext.Session.Remove("UserName");
            return View("LoginDb");
    }


    public IActionResult Logout()
    {
        // Log the user out
        user = null;
        // Redirect to the login page
        return RedirectToAction("LoginDb");

        // Log the user out
        // Redirect to the login page
        //return RedirectToAction("LoginDb");
    }
        //public ActionResult Logout()
        //{
        //FormsAuthentication.SignOut();
        //return RedirectToAction("Index", "Home");
        // }
    }
}


