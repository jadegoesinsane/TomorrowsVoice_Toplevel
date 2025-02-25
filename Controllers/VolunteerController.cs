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
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class VolunteerController : ElephantController
	{
		private readonly TVContext _context;

		public VolunteerController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: Volunteer
		public async Task<IActionResult> Index()
		{
			return View(await _context.Volunteers.ToListAsync());
		}

		// GET: Volunteer/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers
				.FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}


            var userShifts = await _context.UserShifts
			  .Where(u => u.User != null && u.User.ID == id)
			  .ToListAsync();

            // Calculate the total work duration for the volunteer
            TimeSpan totalWorkDuration = TimeSpan.Zero;
            foreach (var userShift in userShifts)
            {
                totalWorkDuration += userShift.EndAt - userShift.StartAt;

				if (userShift.EndAt > userShift.StartAt) volunteer.ParticipationCount++;
            }
            volunteer.HoursVolunteered = (int)totalWorkDuration.TotalHours;
			
            foreach (var userShift in userShifts)
            {
                if(userShift.NoShow==true) volunteer.absences++;

            }
            // You may want to include the total work duration in the ViewBag or directly in the View Model


            return View(volunteer);
		}

		// GET: Volunteer/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Volunteer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					volunteer.ID = _context.GetNextID();
					_context.Add(volunteer);
					await _context.SaveChangesAsync();
					AddSuccessToast(volunteer.NameFormatted);
					//_toastNotification.AddSuccessToastMessage($"{singer.NameFormatted} was successfully created.");
					return RedirectToAction("Details", new { volunteer.ID });
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

			return View(volunteer);
		}

		// GET: Volunteer/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers.Include(g => g.UserShifts).ThenInclude(e => e.Shift)
			   .FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}
			PopulateAssignedEnrollmentData(volunteer);
			return View(volunteer);
		}

		// POST: Volunteer/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions)
		{
			var volunteerToUpdate = await _context.Volunteers.Include(g => g.UserShifts).ThenInclude(e => e.Shift)
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (volunteerToUpdate == null)
			{
				return NotFound();
			}
			UpdateEnrollments(selectedOptions, volunteerToUpdate);
			// Try updating with posted values
			if (await TryUpdateModelAsync<Volunteer>(volunteerToUpdate,
					"",
					r => r.FirstName,
					r => r.LastName,
				   r => r.MiddleName,
				   r => r.Email,
					r => r.Phone,
					r => r.Status))
			{
				try
				{
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{volunteerToUpdate.NameFormatted} was successfully updated.");
					return RedirectToAction("Details", new { volunteerToUpdate.ID });
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

			PopulateAssignedEnrollmentData(volunteerToUpdate);
			return View(volunteerToUpdate);
		}

		// GET: Volunteer/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers
				.FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}

			return View(volunteer);
		}

		// POST: Volunteer/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var volunteer = await _context.Volunteers

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (volunteer != null)
				{
					//_context.Singers.Remove(singer);

					// Here we are archiving a singer instead of deleting them
					volunteer.Status = Status.Archived;
					await _context.SaveChangesAsync();
					AddSuccessToast(volunteer.NameFormatted);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(volunteer);
		}

		private void PopulateAssignedEnrollmentData(Volunteer volunteer)
		{
			//For this to work, you must have Included the child collection in the parent object
			var allOptions = _context.Shifts;
			var currentOptionsHS = new HashSet<int>(volunteer.UserShifts.Select(b => b.ShiftID));
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
						DisplayText = c.EndAt.ToString()
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.EndAt.ToString()
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}

		private void UpdateEnrollments(string[] selectedOptions, Volunteer volunteerToUpdate)
		{
			if (selectedOptions == null)
			{
				volunteerToUpdate.UserShifts = new List<UserShift>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(volunteerToUpdate.UserShifts.Select(b => b.ShiftID));
			foreach (var c in _context.Shifts)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						volunteerToUpdate.UserShifts.Add(new UserShift
						{
							ShiftID = c.ID,
							UserID = volunteerToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						UserShift? enrollmentToRemove = volunteerToUpdate.UserShifts
							.FirstOrDefault(d => d.ShiftID == c.ID);
						if (enrollmentToRemove != null)
						{
							_context.Remove(enrollmentToRemove);
						}
					}
				}
			}
		}

		private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}