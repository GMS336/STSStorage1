using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvStatusController : BaseController
    {
        private readonly STSStorage1Context _context;
        public InvStatusController(STSStorage1Context context)
        {
            _context = context;
        }

        // GET: InvStatus
        public async Task<IActionResult> StatusIndex()
        {
            return View(await _context.InventoryItemStatus.ToListAsync());
        }

        // __________________________________________________________________
        // GET: InvStatus/Details/5
        public async Task<IActionResult> StatusDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invStatus = await _context.InventoryItemStatus
                .FirstOrDefaultAsync(m => m.ItemStatusID == id);
            if (invStatus == null)
            {
                return NotFound();
            }

            return PartialView(invStatus);
        }
        // __________________________________________________________________
        // GET: InvStatusModels/Create
        public IActionResult StatusCreate()
        {
            return PartialView();
        }

        // POST: InvStatusModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemStatusID,ItemStatus,ItemStatusDescription")] 
        InvStatusModel invStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StatusIndex));
            }
            return View(invStatus);
        }

        // __________________________________________________________________
        // GET: InvStatusModels/Edit/5
        public async Task<IActionResult> StatusEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invStatus = await _context.InventoryItemStatus.FindAsync(id);
            if (invStatus == null)
            {
                return NotFound();
            }
            return PartialView(invStatus);
        }

        // POST: InvStatusModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatusEdit(int id, [Bind("ItemStatusID,ItemStatus,ItemStatusDescription")]
        InvStatusModel invStatus)
        {
            if (id != invStatus.ItemStatusID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvStatusModelExists(invStatus.ItemStatusID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(StatusIndex));
            }
            return View(invStatus);
        }

        // __________________________________________________________________
        // GET: InvStatusModels/Delete/5
        public async Task<IActionResult> StatusDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invStatus = await _context.InventoryItemStatus
                .FirstOrDefaultAsync(m => m.ItemStatusID == id);
            if (invStatus == null)
            {
                return NotFound();
            }

            return PartialView(invStatus);
        }

        // POST: InvStatusModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ItemStatusID)
        {
            var invStatus = await _context.InventoryItemStatus.FindAsync(ItemStatusID);
            if (invStatus != null)
            {
                _context.InventoryItemStatus.Remove(invStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(StatusIndex));
        }
        // __________________________________________________________________
        private bool InvStatusModelExists(int ItemStatusID)
        {
            return _context.InventoryItemStatus.Any(e => e.ItemStatusID == ItemStatusID);
        }
    }
}
