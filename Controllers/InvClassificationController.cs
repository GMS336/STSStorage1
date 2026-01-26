using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvClassificationController : BaseController
    {
        private readonly STSStorage1Context _context;
        public InvClassificationController(STSStorage1Context context)
        {
            _context = context;
        }

        // GET: InvClassification
        public async Task<IActionResult> ClassIndex()
        {
            return View(await _context.InventoryClassification.ToListAsync());
        }

        // __________________________________________________________________
        // GET: InvClassification/Details/5
        public async Task<IActionResult> ClassDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invClassification = await _context.InventoryClassification
                .FirstOrDefaultAsync(m => m.ClassificationID == id);
            if (invClassification == null)
            {
                return NotFound();
            }

            return PartialView(invClassification);
        }
        // __________________________________________________________________
        // GET: InvClassificationModels/Create
        public IActionResult ClassCreate()
        {
            return PartialView();
        }

        // POST: InvClassificationModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassificationID,Classification,ClassificationDescription")] InvClassificationModel invClass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ClassIndex));
            }
            return View(invClass);
        }

        // __________________________________________________________________
        // GET: InvClassificationModels/Edit/5
        public async Task<IActionResult> ClassEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invClass = await _context.InventoryClassification.FindAsync(id);
            if (invClass == null)
            {
                return NotFound();
            }
            return PartialView(invClass);
        }

        // POST: InvClassificationModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClassEdit(int id, [Bind("ClassificationID,Classification,ClassificationDescription")] 
        InvClassificationModel invClass)
        {
            if (id != invClass.ClassificationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvClassificationModelExists(invClass.ClassificationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ClassIndex));
            }
            return View(invClass);
        }

        // __________________________________________________________________
        // GET: InvClassificationModels/Delete/5
        public async Task<IActionResult> ClassDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invClass = await _context.InventoryClassification
                .FirstOrDefaultAsync(m => m.ClassificationID == id);
            if (invClass == null)
            {
                return NotFound();
            }

            return PartialView(invClass);
        }

        // POST: InvClassificationModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ClassificationID)
        {
            var invClass = await _context.InventoryClassification.FindAsync(ClassificationID);
            if (invClass != null)
            {
                _context.InventoryClassification.Remove(invClass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ClassIndex));
        }
        // __________________________________________________________________
        private bool InvClassificationModelExists(int ClassificationID)
        {
            return _context.InventoryClassification.Any(e => e.ClassificationID == ClassificationID);
        }
    }
}
