using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.ViewModels;

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
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
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
            Rehearsal rehearsal = new Rehearsal();
            PopulateAttendanceData(rehearsal);
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
            return View();
        }

        // POST: Rehearsal/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RehearsalDate,StartTime,EndTime,Note,ChapterID")] string[] selectedOptions, Rehearsal rehearsal)
        {
            try
            {
                UpdateAttendance(selectedOptions, rehearsal);
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
            PopulateAttendanceData(rehearsal);
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
            PopulateAttendanceData(rehearsal);
            return View(rehearsal);
        }

        // POST: Rehearsal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,string[] selectedOptions ) //"ID,RehearsalDate,StartTime,EndTime,Note,ChapterID"
        {
            var rehearsalToUpdate = await _context.Rehearsals
                .Include(r => r.Chapter)
                .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (rehearsalToUpdate == null)
            {
                return NotFound();
            }
            UpdateAttendance(selectedOptions, rehearsalToUpdate);
            // Try updating with posted values
            if (await TryUpdateModelAsync<Rehearsal>(rehearsalToUpdate,
                    "",
                    r => r.RehearsalDate,
                    r => r.StartTime,
                    r => r.EndTime,
                    r => r.Note,
                    r => r.ChapterID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { rehearsalToUpdate.ID });
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RehearsalExists(rehearsalToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
                    }
                }
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", rehearsalToUpdate.ChapterID);
            PopulateAttendanceData(rehearsalToUpdate);
            return View(rehearsalToUpdate);
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


        private void PopulateAttendanceData(Rehearsal rehearsal)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Singers;
            var currentOptionsHS = new HashSet<int>(rehearsal.RehearsalAttendances.Select(b => b.SingerID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.NameFormatted
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.NameFormatted
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateAttendance(string[] selectedOptions, Rehearsal rehearsalToUpdate)
        {
            if (selectedOptions == null)
            {
                rehearsalToUpdate.RehearsalAttendances = new List<RehearsalAttendance>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(rehearsalToUpdate.RehearsalAttendances.Select(b => b.SingerID));
            foreach (var s in _context.Singers)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(s.ID))//but not currently in the Doctor's collection - Add it!
                    {
                        rehearsalToUpdate.RehearsalAttendances.Add(new RehearsalAttendance
                        {
                            SingerID = s.ID,
                            RehearsalID = rehearsalToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(s.ID))//but is currently in the Doctor's collection - Remove it!
                    {
                        RehearsalAttendance? specToRemove = rehearsalToUpdate.RehearsalAttendances
                            .FirstOrDefault(d => d.SingerID == s.ID);
                        if (specToRemove != null)
                        {
                            _context.Remove(specToRemove);
                        }
                    }
                }
            }
        }


       

        private bool RehearsalExists(int id)
        {
            return _context.Rehearsals.Any(e => e.ID == id);
        }
    }
}
