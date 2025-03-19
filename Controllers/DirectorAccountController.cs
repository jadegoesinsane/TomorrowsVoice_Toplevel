using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	/// <summary>
	/// Specialized controller just used to allow an
	/// Authenticated user to maintain their own account details.
	/// </summary>
	[Authorize]
	public class DirectorAccountController : CognizantController
	{
		private readonly TVContext _context;

		public DirectorAccountController(TVContext context)
		{
			_context = context;
		}

		// GET: DirectorAccount
		public IActionResult Index()
		{
			return RedirectToAction(nameof(Details));
		}

		// GET: DirectorAccount/Details/5
		public async Task<IActionResult> Details()
		{
			var director = await _context.Directors
			   .Where(d => d.Email == User.Identity.Name)
			   .Select(d => new DirectorVM
			   {
				   ID = d.ID,
				   FirstName = d.FirstName,
				   MiddleName = d.MiddleName,
				   LastName = d.LastName,
				   Phone = d.Phone
			   })
			   .FirstOrDefaultAsync();
			if (director == null)
			{
				return NotFound();
			}

			return View(director);
		}

		// GET: DirectorAccount/Edit/5
		public async Task<IActionResult> Edit()
		{
			var director = await _context.Directors
			   .Where(d => d.Email == User.Identity.Name)
			   .Select(d => new DirectorVM
			   {
				   ID = d.ID,
				   FirstName = d.FirstName,
				   MiddleName = d.MiddleName,
				   LastName = d.LastName,
				   Phone = d.Phone
			   })
			   .FirstOrDefaultAsync();
			if (director == null)
			{
				return NotFound();
			}
			return View(director);
		}

		// POST: DirectorAccount/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var directorToUpdate = await _context.Directors
				.FirstOrDefaultAsync(m => m.ID == id);

			//Note: Using TryUpdateModel we do not need to invoke the ViewModel
			//Only allow some properties to be updated
			if (await TryUpdateModelAsync<Director>(directorToUpdate, "",
				c => c.FirstName, c => c.MiddleName, c => c.LastName, c => c.Phone))
			{
				try
				{
					_context.Update(directorToUpdate);
					await _context.SaveChangesAsync();
					UpdateUserNameCookie(directorToUpdate.NameFormatted);
					return RedirectToAction(nameof(Details));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectorExists(directorToUpdate.ID))
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
			return View(directorToUpdate);
		}

		private void UpdateUserNameCookie(string userName)
		{
			CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
		}

		private bool DirectorExists(int id)
		{
			return _context.Directors.Any(e => e.ID == id);
		}
	}
}