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
		public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			try
			{
				if (ModelState.IsValid)
				{
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

			var volunteer = await _context.Volunteers.FindAsync(id);
			if (volunteer == null)
			{
				return NotFound();
			}
			return View(volunteer);
		}

		// POST: Volunteer/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{

			var volunteerToUpdate = await _context.Volunteers
			   
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (volunteerToUpdate == null)
			{
				return NotFound();
			}

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

		private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}