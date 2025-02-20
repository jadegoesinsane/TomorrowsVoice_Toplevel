﻿using System;
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
using TomorrowsVoice_Toplevel.ViewModels;

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

			Shift shift = new Shift();
			PopulateAssignedEnrollmentData(shift);
			
			
			ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name");
            return View();
        }

        // POST: Shift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift, string[] selectedOptions)
		{

			try
			{

				UpdateEnrollments(selectedOptions, shift);
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
			PopulateAssignedEnrollmentData(shift);
			ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name");
			return View(shift);


			
        }

        // GET: Shift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

           

            var shift = await _context.Shifts
               .Include(g => g.VolunteerShifts).ThenInclude(e => e.Volunteer)
               .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }
            PopulateAssignedEnrollmentData(shift);
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name", shift.EventID);
            return View(shift);
        }

        // POST: Shift/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,string[] selectedOptions)
        {

            var shiftToUpdate = await _context.Shifts
               .Include(g => g.VolunteerShifts).ThenInclude(e => e.Volunteer)
               .FirstOrDefaultAsync(m => m.ID == id);

            if (shiftToUpdate == null)
			{
				return NotFound();
			}
            UpdateEnrollments(selectedOptions, shiftToUpdate);
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
            PopulateAssignedEnrollmentData(shiftToUpdate);
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "Name", shiftToUpdate.EventID);
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


		private void PopulateAssignedEnrollmentData(Shift shift)
		{
			//For this to work, you must have Included the child collection in the parent object
			var allOptions = _context.Volunteers ;
			var currentOptionsHS = new HashSet<int>(shift.VolunteerShifts.Select(b => b.UserID));
			//Instead of one list with a boolean, we will make two lists
			var selected = new List<ListOptionVM>();
			var available = new List<ListOptionVM>();
			foreach (var c in allOptions)
			{
				if (currentOptionsHS.Contains(c.ID))
				{
					selected.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.NameFormatted
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.NameFormatted
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}
		private void UpdateEnrollments(string[] selectedOptions, Shift shiftToUpdate)
		{
			if (selectedOptions == null)
			{
				shiftToUpdate.VolunteerShifts = new List<UserShift>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(shiftToUpdate.VolunteerShifts.Select(b => b.UserID));
			foreach (var c in _context.Volunteers)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						shiftToUpdate.VolunteerShifts.Add(new UserShift
						{
							UserID = c.ID,
							ShiftID = shiftToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						UserShift? enrollmentToRemove = shiftToUpdate.VolunteerShifts
							.FirstOrDefault(d => d.UserID == c.ID);
						if (enrollmentToRemove != null)
						{
							_context.Remove(enrollmentToRemove);
						}
					}
				}
			}
		}

		private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
