using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.CustomControllers;
using NToastNotify;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class ShiftSignUpController : ElephantController
    {
        private readonly TVContext _context;

        public ShiftSignUpController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
        {
            _context = context;
        }

        // GET: ShiftSignUp
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Shifts.Include(s => s.Event);
            return View(await tVContext.ToListAsync());
        }

        // GET: ShiftSignUp/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        private SelectList CitySelectList()
        {
            return new SelectList(_context
                .Cities
                .OrderBy(c => c.Name), "ID", "Name");
        }

        public void populateLists()
        {
            ViewData["CityID"] = CitySelectList();
        }

        //// GET: ShiftSignUp/Create
        //public IActionResult Create()
        //{
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID");
        //    return View();
        //}

        //// POST: ShiftSignUp/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(shift);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// GET: ShiftSignUp/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var shift = await _context.Shifts.FindAsync(id);
        //    if (shift == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// POST: ShiftSignUp/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        //{
        //    if (id != shift.ID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(shift);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ShiftExists(shift.ID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// GET: ShiftSignUp/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var shift = await _context.Shifts
        //        .Include(s => s.Event)
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (shift == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(shift);
        //}

        //// POST: ShiftSignUp/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var shift = await _context.Shifts.FindAsync(id);
        //    if (shift != null)
        //    {
        //        _context.Shifts.Remove(shift);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
