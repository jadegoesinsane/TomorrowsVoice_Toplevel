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
    public class SingerController : Controller
    {
        private readonly TVContext _context;

        public SingerController(TVContext context)
        {
            _context = context;
        }

        // GET: Singer
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Singers.Include(s => s.Chapter);
            return View(await tVContext.ToListAsync());
        }

        // GET: Singer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
               .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        // GET: Singer/Create
        public IActionResult Create()
        {
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
            return View();
        }

        // POST: Singer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,ContactName,Phone,Note,ChapterID")] Singer singer)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(singer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
            }

            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singer.ChapterID);
            return View(singer);
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (singer == null)
            {
                return NotFound();
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singer.ChapterID);
            return View(singer);
        }

        // POST: Singer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            var singerToUpdate = await _context.Singers
               .Include(r => r.Chapter)
               .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
               .FirstOrDefaultAsync(m => m.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            // Try updating with posted values
            if (await TryUpdateModelAsync<Singer>(singerToUpdate,
                    "",
                    r => r.FirstName,
                    r => r.LastName,
                    r => r.ContactName,
                    r => r.Phone,
                    r => r.Note,
                    r => r.ContactName,
                    r => r.ChapterID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { singerToUpdate.ID });
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SingerExists(singerToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
                    }
                }
            }


            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singerToUpdate.ChapterID);
            return View(singerToUpdate);
        }

        // GET: Singer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        // POST: Singer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var singer = await _context.Singers
               .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
               .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (singer != null)
                {
                    _context.Singers.Remove(singer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Please try again.");
            }

            return View(singer);
        }

        private bool SingerExists(int id)
        {
            return _context.Singers.Any(e => e.ID == id);
        }
    }
}
