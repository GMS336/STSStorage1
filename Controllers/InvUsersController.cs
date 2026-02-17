using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvUsersController(STSStorage1Context context) : BaseController
    {
        private readonly STSStorage1Context _context = context;

        //_______________________________________________________________________
        // GET: InvUsers/ProfileEdit/5
        /// <summary>
        /// Loads the profile edit page for the specified user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>ProfileEdit view with user data</returns>
        public async Task<IActionResult> ProfileEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invUsers = await _context.InventoryUsers.FindAsync(id);
            if (invUsers == null)
            {
                return NotFound();
            }

            // FIX: Fetch and populate the role name for display in the view
            var role = await _context.InventoryRole
                .FirstOrDefaultAsync(r => r.RoleId == invUsers.Role_Id);

            if (role != null)
            {
                ViewBag.RoleName = role.RoleName;
            }
            else
            {
                ViewBag.RoleName = "Not Assigned";
            }

            return View("ProfileEdit", invUsers);
        }

        // POST: InvUsers/ProfileEdit/5
        /// <summary>
        /// Saves profile edits for the specified user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileEdit(int id, [Bind("MyID,FirstName,LastName,EmailAddress,PhoneNum,UserPlant,UserFunction,UserDept,UserName,Password,Role_Id")]
        InvUsersModel invUsers)
        {
            if (id != invUsers.MyID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invUsers);
                    await _context.SaveChangesAsync();

                    // Success - redirect to home
                    return RedirectToAction(nameof(HomeController.STSHome), "Home");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvUserExists(invUsers.MyID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // FIX: If validation fails, re-fetch the role name before returning to view
            var role = await _context.InventoryRole
                .FirstOrDefaultAsync(r => r.RoleId == invUsers.Role_Id);

            if (role != null)
            {
                ViewBag.RoleName = role.RoleName;
            }
            else
            {
                ViewBag.RoleName = "Not Assigned";
            }

            return View("ProfileEdit", invUsers);
        }

        // __________________________________________________________________
        // GET: InvUsers/UserIndex
        /// <summary>
        /// Displays list of all users with their roles
        /// </summary>
        public async Task<IActionResult> UserIndex(string? sortOrder)
        {
            // Join User table with Role table
            var userRoles = await (from user in _context.InventoryUsers
                                   join role in _context.InventoryRole
                                   on user.Role_Id equals role.RoleId
                                   select new UserRoleViewModel
                                   {
                                       MyID = user.MyID,
                                       FirstName = user.FirstName,
                                       LastName = user.LastName,
                                       EmailAddress = user.EmailAddress,
                                       UserFunction = user.UserFunction,
                                       UserDept = user.UserDept,
                                       RoleName = role.RoleName
                                   }).ToListAsync();

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "FirstName":
                    userRoles = userRoles.OrderBy(u => u.FirstName).ToList();
                    break;
                case "UserFunction":
                    userRoles = userRoles.OrderByDescending(u => u.UserFunction).ToList();
                    break;
                case "UserDept":
                    userRoles = userRoles.OrderBy(u => u.UserDept).ToList();
                    break;
                case "RoleName":
                    userRoles = userRoles.OrderBy(u => u.RoleName).ToList();
                    break;
                default:
                    userRoles = userRoles.OrderBy(u => u.LastName).ToList();
                    break;
            }

            return View(userRoles);
        }

        // __________________________________________________________________
        // GET: InvUsers/UserDetails/5
        /// <summary>
        /// Shows detailed information for a specific user
        /// </summary>
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Join User with Role to fetch RoleName
            var employeeDetail = await (from user in _context.InventoryUsers
                                        join role in _context.InventoryRole
                                        on user.Role_Id equals role.RoleId
                                        where user.MyID == id
                                        select new UserRoleViewModel
                                        {
                                            MyID = user.MyID,
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            EmailAddress = user.EmailAddress,
                                            UserFunction = user.UserFunction,
                                            UserDept = user.UserDept,
                                            RoleName = role.RoleName
                                        }).FirstOrDefaultAsync();

            if (employeeDetail == null)
            {
                return NotFound();
            }

            return PartialView(employeeDetail);
        }

        // __________________________________________________________________
        // GET: InvUsers/UserCreate
        /// <summary>
        /// Shows the create new user form
        /// </summary>
        public IActionResult UserCreate()
        {
            // Populate roles dropdown for create form
            IEnumerable<SelectListItem> roles = _context.InventoryRole
                .Select(c => new SelectListItem
                {
                    Value = c.RoleId.ToString(),
                    Text = c.RoleName
                });

            ViewBag.Roles = roles;

            return PartialView();
        }

        // POST: InvUsers/Create
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MyID,FirstName,LastName,EmailAddress,PhoneNum,UserPlant,UserFunction,UserDept,UserName,Password,Role_Id")]
        InvUsersModel invUsers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invUsers);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(UserIndex));
            }

            // If validation fails, re-populate the roles dropdown
            IEnumerable<SelectListItem> roles = _context.InventoryRole
                .Select(c => new SelectListItem
                {
                    Value = c.RoleId.ToString(),
                    Text = c.RoleName
                }).ToList();

            ViewBag.Roles = roles;

            return View(invUsers);
        }

        //____________________________________________________________________________________________
        // GET: InvUsers/UserEdit/5
        /// <summary>
        /// Shows the edit form for a specific user (Admin use)
        /// </summary>
        public async Task<IActionResult> UserEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invUsers = await _context.InventoryUsers.FindAsync(id);
            if (invUsers == null)
            {
                return NotFound();
            }

            // Populate the dropdown list with roles
            var roles = _context.InventoryRole.Select(r => new
            {
                Value = r.RoleId,
                Text = r.RoleName
            }).ToList();

            // Create a SelectList and set the selected value to the user's RoleId
            ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);

            return PartialView("UserEdit", invUsers);
        }

        // POST: InvUsers/UserEdit/5
        /// <summary>
        /// Saves edits for a specific user (Admin use)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(int id, [Bind("MyID,FirstName,LastName,EmailAddress,PhoneNum,UserPlant,UserFunction,UserDept,UserName,Password,Role_Id")]
        InvUsersModel invUsers)
        {
            if (id != invUsers.MyID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invUsers);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(UserIndex));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvUserExists(invUsers.MyID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // FIX: If ModelState is invalid, re-populate the dropdown list for the view
            var roles = _context.InventoryRole.Select(r => new
            {
                Value = r.RoleId,
                Text = r.RoleName
            }).ToList();
            ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);

            return View("UserEdit", invUsers);
        }

        //________________________________________________________________________________________
        // GET: InvUsers/UserDelete/5
        /// <summary>
        /// Shows confirmation dialog for deleting a user
        /// </summary>
        public async Task<IActionResult> UserDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Join User with Role to fetch RoleName for display
            var userToDelete = await (from invuser in _context.InventoryUsers
                                      join invrole in _context.InventoryRole
                                      on invuser.Role_Id equals invrole.RoleId
                                      where invuser.MyID == id
                                      select new UserRoleViewModel
                                      {
                                          MyID = invuser.MyID,
                                          FirstName = invuser.FirstName,
                                          LastName = invuser.LastName,
                                          EmailAddress = invuser.EmailAddress,
                                          PhoneNum = invuser.PhoneNum,
                                          UserFunction = invuser.UserFunction,
                                          UserDept = invuser.UserDept,
                                          RoleName = invrole.RoleName
                                      }).FirstOrDefaultAsync();

            if (userToDelete == null)
            {
                return NotFound();
            }

            return PartialView(userToDelete);
        }

        // POST: InvUsers/Delete/5
        /// <summary>
        /// Deletes the specified user from the database
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int MyID)
        {
            var invUsers = await _context.InventoryUsers.FindAsync(MyID);
            if (invUsers != null)
            {
                _context.InventoryUsers.Remove(invUsers);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(UserIndex));
        }

        /// <summary>
        /// Helper method to check if a user exists in the database
        /// </summary>
        private bool InvUserExists(int id)
        {
            return _context.InventoryUsers.Any(e => e.MyID == id);
        }
    }
}