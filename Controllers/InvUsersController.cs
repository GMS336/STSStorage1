using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
        ///   intellisence comment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

            return View("ProfileEdit", invUsers);

        }

        // POST: EmployeesController/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                return RedirectToAction(nameof(HomeController.STSHome), "Home");
            }
            return View("ProfileEdit", invUsers);
        }












        // __________________________________________________________________
        // GET: UsersController
        public async Task<IActionResult> UserIndex(string? sortOrder)
        //{
        //    return View(await _context.InventoryUsers.ToListAsync());
        //}
        {
            // Join Employee table with Role table
            var userRoles = await (from user in _context.InventoryUsers
                                       join role in _context.InventoryRole
                                       on user.Role_Id equals role.RoleId // Replace with actual FK and PK
                                       select new UserRoleViewModel
                                       {
                                           MyID = user.MyID, // Replace with Employee PK
                                           FirstName = user.FirstName, // Replace with Employee Name property
                                           LastName = user.LastName,
                                           EmailAddress = user.EmailAddress,
                                           UserFunction = user.UserFunction,
                                           UserDept = user.UserDept,
                                           RoleName = role.RoleName // Replace with Role Name property
                                       }).ToListAsync();


            // Apply sorting based on the sortOrder parameter
            // Suggested type of user input
            switch (sortOrder)
            {
                case "FirstName":
                    userRoles = (List<UserRoleViewModel>)userRoles.OrderBy(u => u.FirstName).ToList();
                    break;
                case "UserFunction":
                    userRoles = (List<UserRoleViewModel>)userRoles.OrderByDescending(u => u.UserFunction).ToList();
                    break;
                case "UserDept":
                    userRoles = (List<UserRoleViewModel>)userRoles.OrderBy(u => u.UserDept).ToList();
                    break;
                case "RoleName":
                    userRoles = (List<UserRoleViewModel>)userRoles.OrderBy(u => u.RoleName).ToList();
                    break;
                default:
                    userRoles = (List<UserRoleViewModel>)userRoles.OrderBy(u => u.LastName).ToList();
                    break;
            }

            return View(userRoles);
        }

        // __________________________________________________________________
        // GET: InvUsers/Details/5
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Join Employee with Role to fetch RoleName
            var employeeDetail = await (from user in _context.InventoryUsers
                                        join role in _context.InventoryRole
                                        on user.Role_Id equals role.RoleId // Replace with actual FK and PK
                                        select new UserRoleViewModel
                                        {
                                            MyID = user.MyID, // Replace with Employee PK
                                            FirstName = user.FirstName, // Replace with Employee Name property
                                            LastName = user.LastName,
                                            EmailAddress = user.EmailAddress,
                                            UserFunction = user.UserFunction,
                                            UserDept = user.UserDept,
                                            RoleName = role.RoleName // Replace with Role Name property
                                        }).FirstOrDefaultAsync();


            //var invUsers = await _context.InventoryUsers
            //    .FirstOrDefaultAsync(m => m.MyID == id);
            if (employeeDetail == null)
            {
                return NotFound();
            }

            return PartialView(employeeDetail);
        }

        // __________________________________________________________________
        // GET: InvUsers/Create
        public IActionResult UserCreate()
        {

            IEnumerable<SelectListItem> roles = _context.InventoryRole
            .Select(c => new SelectListItem
            {
                Value = c.RoleId.ToString(),
                Text = c.RoleName
            });

            // Pass the categories to the ViewBag
            ViewBag.Roles = roles;

            // Return the Create view


            return PartialView();
        }

        // POST: EmployeesController/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            // Repopulate the dropdown in case of validation error
            ViewBag.Roles = _context.InventoryRole.Select(c => new SelectListItem
            {
                Value = c.RoleId.ToString(),
                Text = c.RoleName
            }).ToList();

            return View(invUsers);
        }
//____________________________________________________________________________________________
        // GET: InvUsers/Edit/5
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
            // Fetch roles for the dropdown
            var roles = _context.InventoryRole.Select(r => new
            {
                Value = r.RoleId,     // Role ID (Foreign Key)
                Text = r.RoleName   // Role Name
            }).ToList();

            // Create a SelectList and set the selected value to the employee's RoleId
            ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);


            return PartialView("UserEdit", invUsers);

        }

        // POST: EmployeesController/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                // If ModelState is invalid, re-populate the dropdown list for the view
                var roles = _context.InventoryRole.Select(r => new
                {
                    Value = r.RoleId,
                    Text = r.RoleName
                }).ToList();
                ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);

                return RedirectToAction(nameof(UserIndex));
            }
            return View("UserEdit", invUsers);
        }

//________________________________________________________________________________________
        // GET: EmployeesController/Delete/5
        public async Task<IActionResult> UserDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Join Employee with Role to fetch RoleName
            var UserDelete = await (from invuser in _context.InventoryUsers
                                        join invrole in _context.InventoryRole
                                        on invuser.Role_Id equals invrole.RoleId // Replace with actual FK and PK
                                    where invuser.MyID == id // Match the employee by ID
                                    select new UserRoleViewModel
                                        {
                                            MyID = invuser.MyID, // Replace with Employee PK
                                            FirstName = invuser.FirstName, // Replace with Employee Name property
                                            LastName = invuser.LastName,
                                            EmailAddress = invuser.EmailAddress,
                                            PhoneNum = invuser.PhoneNum,
                                            UserFunction = invuser.UserFunction,
                                            UserDept = invuser.UserDept,
                                            RoleName = invrole.RoleName // Replace with Role Name property
                                        }).FirstOrDefaultAsync();

          
            if (UserDelete == null)
            {
                return NotFound();
            }

            return PartialView(UserDelete);
        }

        // POST: EmployeesController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int MyID)
        {
            var invUsers = await _context.InventoryUsers.FindAsync(MyID);
            if (invUsers != null)
            {
                _context.InventoryUsers.Remove(invUsers);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserIndex));
        }

        private bool InvUserExists(int id)
        {
            return _context.InventoryUsers.Any(e => e.MyID == id);
        }
    }
}
