using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class ShiftController : ElephantController
	{
        private readonly TVContext _context;
        
        public ShiftController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
            _context = context;
        }

        // GET: Shift
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Shifts.Include(s => s.Event);
            return View(await tVContext.ToListAsync());
        }

        // GET: Shift/Details/5
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

        // GET: Shift/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name");
            return View();
        }

        // POST: Shift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        {

			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(shift);
					await _context.SaveChangesAsync();
					AddSuccessToast(shift.ShiftDuration.ToString());
					//_toastNotification.AddSuccessToastMessage($"{singer.NameFormatted} was successfully created.");
					return RedirectToAction("Details", new { shift.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("volunteer.Email"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, " +
						"you cannot have duplicate Name and Email.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}


			return View(shift);


			
        }

        // GET: Shift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name", shift.EventID);
            return View(shift);
        }

        // POST: Shift/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        {
			var shiftToUpdate = await _context.Shifts

			   .FirstOrDefaultAsync(m => m.ID == id);

			if (shiftToUpdate == null)
			{
				return NotFound();
			}

			// Try updating with posted values
			if (await TryUpdateModelAsync<Shift>(shiftToUpdate,
					"",
					r => r.StartAt,
					r => r.EndAt,
				   r => r.VolunteersNeeded,
				   r => r.EventID
					))
			{
				try
				{
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{shiftToUpdate.ShiftDuration} was successfully updated.");
					return RedirectToAction("Details", new { shiftToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE") && message.Contains("Volunteers.Email"))
					{
						ModelState.AddModelError("", "Unable to save changes. Remember, " +
							"you cannot have duplicate Name and Email.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}


			return View(shiftToUpdate);



			
        }

        // GET: Shift/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Shift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			var shift = await _context.Shifts

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (shift != null)
				{
					//_context.Singers.Remove(singer);

					// Here we are archiving a singer instead of deleting them
					_context.Shifts.Remove(shift);
					await _context.SaveChangesAsync();
					AddSuccessToast(shift.ShiftDuration.ToString());
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(shift);




        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
