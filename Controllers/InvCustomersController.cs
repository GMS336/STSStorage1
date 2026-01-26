using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvCustomersController(STSStorage1Context context) : BaseController
    {
        private readonly STSStorage1Context _context = context;

        // __________________________________________________________________
        // GET: InvCustomers
        public async Task<IActionResult> CustIndex()
        {
            return View(await _context.InventoryCustomer.ToListAsync());
        }

        // __________________________________________________________________
        // GET: InvCustomers/Details/5
        public async Task<IActionResult> CustDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invCustomer = await _context.InventoryCustomer
                .FirstOrDefaultAsync(m => m.CustomerRecID == id);
            if (invCustomer == null)
            {
                return NotFound();
            }

            return PartialView(invCustomer);
            //return View(invCustomer);
        }

        // __________________________________________________________________
        // GET: InvCustomers/Create
        public IActionResult CustCreate()
        {
            return PartialView();
        }

        // POST: InvCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerRecID,CustomerName,CustomerCode,CustomerLocation")] InvCustomerModel invCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invCustomer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CustIndex));
            }
            return View(invCustomer);
        }

        // __________________________________________________________________
        // GET: InvCustomers/Edit/5
        public async Task<IActionResult> CustEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invCustomer = await _context.InventoryCustomer.FindAsync(id);
            if (invCustomer == null)
            {
                return NotFound();
            }
            return PartialView(invCustomer);
        }

        // POST: InvCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustEdit(int id, [Bind("CustomerRecID,CustomerName,CustomerCode,CustomerLocation")] 
        InvCustomerModel invCustomer)
        {
            if (id != invCustomer.CustomerRecID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvCustomerExists(invCustomer.CustomerRecID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CustIndex));
            }
            return View(invCustomer);
        }

        // __________________________________________________________________
        // GET: InvCustomers/Delete/5
        public async Task<IActionResult> CustDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invCustomer = await _context.InventoryCustomer
                .FirstOrDefaultAsync(m => m.CustomerRecID == id);
            if (invCustomer == null)
            {
                return NotFound();
            }

            return PartialView(invCustomer);
        }

        // POST: InvCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int CustomerRecID)
        {
            var invCustomer = await _context.InventoryCustomer.FindAsync(CustomerRecID);
            if (invCustomer != null)
            {
                _context.InventoryCustomer.Remove(invCustomer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CustIndex));
        }

        private bool InvCustomerExists(int CustomerRecID)
        {
            return _context.InventoryCustomer.Any(e => e.CustomerRecID == CustomerRecID);
        }
    }
}
