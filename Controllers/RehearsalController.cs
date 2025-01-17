using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class RehearsalController : Controller
    {
        private readonly TVContext _context;

        public RehearsalController(TVContext context)
        {
            _context = context;
        }

        // GET: Rehearsal
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Rehearsals.Include(r => r.Chapter);
            return View(await tVContext.ToListAsync());
        }

        // GET: Rehearsal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r=>r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rehearsal == null)
            {
                return NotFound();
            }

            return View(rehearsal);
        }

        // GET: Rehearsal/Create
        public IActionResult Create()
        {
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
            return View();
        }

        // POST: Rehearsal/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartTime,EndTime,Note,ChapterID")] Rehearsal rehearsal)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(rehearsal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
            }
            
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", rehearsal.ChapterID);
            return View(rehearsal);
        }

        // GET: Rehearsal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rehearsal == null)
            {
                return NotFound();
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", rehearsal.ChapterID);
            return View(rehearsal);
        }

        // POST: Rehearsal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id) //"Id,StartTime,EndTime,Note,ChapterID"
        {
            var RehearsalToUpdate = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (RehearsalToUpdate == null)
            {
                return NotFound();
            }

            // Try updating with posted values
            if (await TryUpdateModelAsync<Rehearsal>(RehearsalToUpdate, 
                    "", 
                    r=>r.StartTime, 
                    r=>r.EndTime, 
                    r=>r.Note, 
                    r=>r.ChapterID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new {RehearsalToUpdate.ID});
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RehearsalExists(RehearsalToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
                    }
                }
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", RehearsalToUpdate.ChapterID);
            return View(RehearsalToUpdate);
        }

        // GET: Rehearsal/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rehearsal = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rehearsal == null)
            {
                return NotFound();
            }

            return View(rehearsal);
        }

        // POST: Rehearsal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rehearsal = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (rehearsal != null)
                {
                    _context.Rehearsals.Remove(rehearsal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Please try again.");
            }
            return View(rehearsal);
        }

        private bool RehearsalExists(int id)
        {
            return _context.Rehearsals.Any(e => e.ID == id);
        }
    }
}
