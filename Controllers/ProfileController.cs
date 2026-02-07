using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class ProfileController(STSStorage1Context context) : BaseController
    {
        private readonly STSStorage1Context _context = context;

        //____________________________________________________________________________________________
        // GET: InvUsers/Edit/5
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

            // Populate the dropdown list with roles
            // Fetch roles for the dropdown
            var roles = _context.InventoryRole.Select(r => new
            {
                Value = r.RoleId,     // Role ID (Foreign Key)
                Text = r.RoleName   // Role Name
            }).ToList();

            // Create a SelectList and set the selected value to the employee's RoleId
            ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);


            return View();

        }

        // POST: ProfileController/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
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
                // If ModelState is invalid, re-populate the dropdown list for the view
                var roles = _context.InventoryRole.Select(r => new
                {
                    Value = r.RoleId,
                    Text = r.RoleName
                }).ToList();
                ViewBag.Roles = new SelectList(roles, "Value", "Text", invUsers.Role_Id);

                return RedirectToAction("STSHome", "Home");
            }
            return View("UserEdit", invUsers);
        }


        private bool InvUserExists(int id)
        {
            return _context.InventoryUsers.Any(e => e.MyID == id);
        }
    }
}
