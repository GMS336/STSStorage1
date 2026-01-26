using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class InvPhaseController : BaseController
    {
        private readonly STSStorage1Context _context;
        public InvPhaseController(STSStorage1Context context)
        {
            _context = context;
        }

        // GET: InvPhase
        public async Task<IActionResult> PhaseIndex()
        {
            return View(await _context.InventoryProjectPhase.ToListAsync());
        }

        // __________________________________________________________________
        // GET: InvPhase/Details/5
        public async Task<IActionResult> PhaseDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invPhase = await _context.InventoryProjectPhase
                .FirstOrDefaultAsync(m => m.ProgramPhaseID == id);
            if (invPhase == null)
            {
                return NotFound();
            }

            return PartialView(invPhase);
        }
        // __________________________________________________________________
        // GET: InvPhaseModels/Create
        public IActionResult PhaseCreate()
        {
            return PartialView();
        }

        // POST: InvPhaseModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgramPhaseID,PhaseName,PhaseDescription")] 
        InvPhaseModel invPhase)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invPhase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PhaseIndex));
            }
            return View(invPhase);
        }

        // __________________________________________________________________
        // GET: InvPhaseModels/Edit/5
        public async Task<IActionResult> PhaseEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invPhase = await _context.InventoryProjectPhase.FindAsync(id);
            if (invPhase == null)
            {
                return NotFound();
            }
            return PartialView(invPhase);
        }

        // POST: InvPhaseModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhaseEdit(int id, [Bind("ProgramPhaseID,PhaseName,PhaseDescription")]
        InvPhaseModel invPhase)
        {
            if (id != invPhase.ProgramPhaseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invPhase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvPhaseModelExists(invPhase.ProgramPhaseID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(PhaseIndex));
            }
            return View(invPhase);
        }

        // __________________________________________________________________
        // GET: InvPhaseModels/Delete/5
        public async Task<IActionResult> PhaseDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invPhase = await _context.InventoryProjectPhase
                .FirstOrDefaultAsync(m => m.ProgramPhaseID == id);
            if (invPhase == null)
            {
                return NotFound();
            }

            return PartialView(invPhase);
        }

        // POST: InvPhaseModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ProgramPhaseID)
        {
            var invPhase = await _context.InventoryProjectPhase.FindAsync(ProgramPhaseID);
            if (invPhase != null)
            {
                _context.InventoryProjectPhase.Remove(invPhase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PhaseIndex));
        }
        // __________________________________________________________________
        private bool InvPhaseModelExists(int ProgramPhaseID)
        {
            return _context.InventoryProjectPhase.Any(e => e.ProgramPhaseID == ProgramPhaseID);
        }
    }
}
