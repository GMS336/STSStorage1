using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvShelfController : BaseController
    {
        private readonly STSStorage1Context _context;
        public InvShelfController(STSStorage1Context context)
        {
            _context = context;
        }

        // GET: InvShelf
        public async Task<IActionResult> ShelfIndex()
        {
            return View(await _context.InventoryShelf.ToListAsync());
        }

        // __________________________________________________________________
        // GET: InvShelf/Details/5
        public async Task<IActionResult> ShelfDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invShelf = await _context.InventoryShelf
                .FirstOrDefaultAsync(m => m.ShelfRecid == id);
            if (invShelf == null)
            {
                return NotFound();
            }

            return PartialView(invShelf);
        }
        // __________________________________________________________________
        // GET: InvShelfModels/Create
        public IActionResult ShelfCreate()
        {
            return PartialView();
        }

        // POST: InvShelfModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShelfRecid,ShelfName,ShelfDescription,StorageLocationSite")] 
        InvShelfModel invShelf)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invShelf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ShelfIndex));
            }
            return View(invShelf);
        }

        // __________________________________________________________________
        // GET: InvShelfModels/Edit/5
        public async Task<IActionResult> ShelfEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invShelf = await _context.InventoryShelf.FindAsync(id);
            if (invShelf == null)
            {
                return NotFound();
            }
            return PartialView(invShelf);
        }

        // POST: InvShelfModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShelfEdit(int id, [Bind("ShelfRecid,ShelfName,ShelfDescription,StorageLocationSite")]
        InvShelfModel invShelf)
        {
            if (id != invShelf.ShelfRecid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invShelf);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvShelfModelExists(invShelf.ShelfRecid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ShelfIndex));
            }
            return View(invShelf);
        }

        // __________________________________________________________________
        // GET: InvShelfModels/Delete/5
        public async Task<IActionResult> ShelfDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invShelf = await _context.InventoryShelf
                .FirstOrDefaultAsync(m => m.ShelfRecid == id);
            if (invShelf == null)
            {
                return NotFound();
            }

            return PartialView(invShelf);
        }

        // POST: InvShelfModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ShelfRecid)
        {
            var invShelf = await _context.InventoryShelf.FindAsync(ShelfRecid);
            if (invShelf != null)
            {
                _context.InventoryShelf.Remove(invShelf);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ShelfIndex));
        }
        // __________________________________________________________________
        private bool InvShelfModelExists(int ShelfRecid)
        {
            return _context.InventoryShelf.Any(e => e.ShelfRecid == ShelfRecid);
        }
    }
}
