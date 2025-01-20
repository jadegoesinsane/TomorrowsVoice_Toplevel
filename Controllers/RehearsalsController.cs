using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class RehearsalsController : Controller
    {
        private readonly TVContext _context;

        public RehearsalsController(TVContext context)
        {
            _context = context;
        }

        // GET: Rehearsals
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Rehearsals.Include(r => r.Director);
            return View(await tVContext.ToListAsync());
        }

        // GET: Rehearsals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals
                .Include(r => r.Director)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rehearsal == null)
            {
                return NotFound();
            }

            return View(rehearsal);
        }

        // GET: Rehearsals/Create
        public IActionResult Create()
        {
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Summary");
            return View();
        }

        // POST: Rehearsals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RehearsalDate,StartTime,EndTime,Note,DirectorID")] Rehearsal rehearsal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rehearsal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Summary", rehearsal.DirectorID);
            return View(rehearsal);
        }

        // GET: Rehearsals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals.FindAsync(id);
            if (rehearsal == null)
            {
                return NotFound();
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", rehearsal.DirectorID);
            return View(rehearsal);
        }

        // POST: Rehearsals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RehearsalDate,StartTime,EndTime,Note,DirectorID")] Rehearsal rehearsal)
        {
            if (id != rehearsal.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rehearsal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RehearsalExists(rehearsal.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", rehearsal.DirectorID);
            return View(rehearsal);
        }

        // GET: Rehearsals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals
                .Include(r => r.Director)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rehearsal == null)
            {
                return NotFound();
            }

            return View(rehearsal);
        }

        // POST: Rehearsals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rehearsal = await _context.Rehearsals.FindAsync(id);
            if (rehearsal != null)
            {
                _context.Rehearsals.Remove(rehearsal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RehearsalExists(int id)
        {
            return _context.Rehearsals.Any(e => e.ID == id);
        }
    }
}
