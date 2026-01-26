using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvRegisterController(STSStorage1Context context) : BaseController
    {
        private readonly STSStorage1Context _context = context;

        // __________________________________________________________________
        // GET: UsersController
        //public async Task<IActionResult> RegIndex(/*string? sortOrder*/)
        //{
        //    return View(await _context.InventoryRegister.ToListAsync());
        //}
        //{
        //    if (sortOrder == "FirstName")
        //    {
        //        return View(await _context.InventoryRegister.OrderBy(r => r.FirstName).ToListAsync());
        //    }
        //    else if (sortOrder == "UserFunction")
        //    {
        //        return View(await _context.InventoryRegister.OrderBy(r => r.UserFunction).ToListAsync());
        //    }
        //    else if (sortOrder == "UserDept")
        //    {
        //        return View(await _context.InventoryRegister.OrderBy(r => r.UserDept).ToListAsync());
        //    }
        //    else if (sortOrder == "InventoryRole")
        //    {
        //        return View(await _context.InventoryRegister.OrderBy(r => r.InventoryRole).ToListAsync());
        //    }
        //    else
        //    {
        //        return View(await _context.InventoryRegister.OrderBy(r => r.LastName).ToListAsync());
        //    }
        //}

        // __________________________________________________________________
        // GET: InvUsers/Details/5
        //public async Task<IActionResult> RegDetails(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invRegister = await _context.InventoryRegister
        //        .FirstOrDefaultAsync(m => m.MyID == id);
        //    if (invRegister == null)
        //    {
        //        return NotFound();
        //    }

        //    return PartialView(invRegister);
        //}

        // __________________________________________________________________
        // GET: InvUsers/Create
        public IActionResult RegCreate()
        {
            return PartialView();
        }

        // POST: EmployeesController/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MyID,FirstName,LastName,EmailAddress,PhoneNum,InventoryRole,UserPlant,UserFunction,UserDept")]
        InvUsersModel invRegister)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invRegister);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(LoginDb));
            }
            return View(invRegister);
        }
        //____________________________________________________________________________________________
        // GET: InvUsers/Edit/5
        //public async Task<IActionResult> RegEdit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invRegister = await _context.InventoryRegister.FindAsync(id);
        //    if (invRegister == null)
        //    {
        //        return NotFound();
        //    }
        //    return PartialView(invRegister);
        //}

        // POST: EmployeesController/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegEdit(int id, [Bind("MyID,FirstName,LastName,EmailAddress,PhoneNum,InventoryRole,UserPlant,UserFunction,UserDept")]
        //InvUsersModel invRegister)
        //{
        //    if (id != invRegister.MyID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(invRegister);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!InvRegisterExists(invRegister.MyID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(RegIndex));
        //    }
        //    return View(invRegister);
        //}

        //________________________________________________________________________________________
        // GET: EmployeesController/Delete/5
        //public async Task<IActionResult> UserDelete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invRegister = await _context.InventoryRegister
        //        .FirstOrDefaultAsync(m => m.MyID == id);
        //    if (invRegister == null)
        //    {
        //        return NotFound();
        //    }

        //    return PartialView(invRegister);
        //}

        // POST: EmployeesController/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int MyID)
        //{
        //    var invRegister = await _context.InventoryRegister.FindAsync(MyID);
        //    if (invRegister != null)
        //    {
        //        _context.InventoryRegister.Remove(invRegister);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(RegIndex));
        //}

        private bool InvRegisterExists(int id)
        {
            return _context.InventoryRegister.Any(e => e.MyID == id);
        }
    }
}
