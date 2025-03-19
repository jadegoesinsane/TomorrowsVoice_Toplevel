using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	/// <summary>
	/// Specialized controller just used to allow an
	/// Authenticated user to maintain their own account details.
	/// </summary>
	[Authorize]
	public class VolunteerAccountController : CognizantController
	{
		private readonly TVContext _context;

		public VolunteerAccountController(TVContext context)
		{
			_context = context;
		}

		// GET: VolunteerAccount
		public IActionResult Index()
		{
			return RedirectToAction(nameof(Details));
		}

		// GET: VolunteerAccount/Details/5
		public async Task<IActionResult> Details()
		{
			var volunteer = await _context.Volunteers
			   .Where(v => v.Email == User.Identity.Name)
			   .Select(v => new VolunteerVM
			   {
				   ID = v.ID,
				   FirstName = v.FirstName,
				   MiddleName = v.MiddleName,
				   LastName = v.LastName,
				   Phone = v.Phone,
				   YearlyVolunteerGoal = v.YearlyVolunteerGoal
			   })
			   .FirstOrDefaultAsync();
			if (volunteer == null)
			{
				return NotFound();
			}

			return View(volunteer);
		}

		// GET: VolunteerAccount/Edit/5
		public async Task<IActionResult> Edit()
		{
			var volunteer = await _context.Volunteers
			   .Where(v => v.Email == User.Identity.Name)
			   .Select(v => new VolunteerVM
			   {
				   ID = v.ID,
				   FirstName = v.FirstName,
				   MiddleName = v.MiddleName,
				   LastName = v.LastName,
				   Phone = v.Phone,
				   YearlyVolunteerGoal = v.YearlyVolunteerGoal
			   })
			   .FirstOrDefaultAsync();
			if (volunteer == null)
			{
				return NotFound();
			}
			return View(volunteer);
		}

		// POST: VolunteerAccount/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var volunteerToUpdate = await _context.Volunteers
				.FirstOrDefaultAsync(m => m.ID == id);

			//Note: Using TryUpdateModel we do not need to invoke the ViewModel
			//Only allow some properties to be updated
			if (await TryUpdateModelAsync<Volunteer>(volunteerToUpdate, "",
				c => c.FirstName, c => c.MiddleName, c => c.LastName, c => c.Phone, c => c.YearlyVolunteerGoal))
			{
				try
				{
					_context.Update(volunteerToUpdate);
					await _context.SaveChangesAsync();
					UpdateUserNameCookie(volunteerToUpdate.NameFormatted);
					return RedirectToAction(nameof(Details));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!VolunteerExists(volunteerToUpdate.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (DbUpdateException)
				{
					//Since we do not allow changing the email, we cannot introduce a duplicate
					ModelState.AddModelError("", "Something went wrong in the database.");
				}
			}
			return View(volunteerToUpdate);
		}

		private void UpdateUserNameCookie(string userName)
		{
			CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
		}

		private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}