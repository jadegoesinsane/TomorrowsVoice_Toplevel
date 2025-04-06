using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
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
		private readonly IToastNotification _toastNotification;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		public VolunteerAccountController(
	   TVContext context,
	   IToastNotification toastNotification,
	   UserManager<IdentityUser> userManager,
	   SignInManager<IdentityUser> signInManager)  // 注入 SignInManager
		{
			_context = context;
			_toastNotification = toastNotification;
			_userManager = userManager;
			_signInManager = signInManager;  // 赋值
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
				   YearlyVolunteerGoal = v.YearlyVolunteerGoal,
				   Email = v.Email,
				   absences = v.absences,
				   ParticipationCount = v.ParticipationCount,
				   TotalWorkDuration = v.TotalWorkDuration
			   })
			   .FirstOrDefaultAsync();
			if (volunteer == null)
			{
				return RedirectToAction("Create");
			}
			return View(volunteer);
		}

		// GET: Volunteer/Create
		public IActionResult Create()
		{
			var volunteer = new VolunteerVM { Email = User.Identity.Name };
			return View(volunteer);
		}

		// POST: Volunteer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Phone,Email,YearlyVolunteerGoal")] VolunteerVM vVM)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Volunteer volunteer = new Volunteer
					{
						FirstName = vVM.FirstName,
						MiddleName = vVM.MiddleName,
						LastName = vVM.LastName,
						Phone = vVM.Phone,
						YearlyVolunteerGoal = vVM.YearlyVolunteerGoal,
						Email = User.Identity.Name
					};

					_context.Add(volunteer);
					await _context.SaveChangesAsync();

					var _user = await _userManager.FindByEmailAsync(volunteer.Email);
					if (_user != null)
					{
						var userRoles = await _userManager.GetRolesAsync(_user);
						if (!userRoles.Contains("Volunteer"))
						{
							await _userManager.AddToRoleAsync(_user, "Volunteer");
						}

						await _signInManager.RefreshSignInAsync(_user);
					}

					_toastNotification.AddSuccessToastMessage($"Finished setting up account, welcome {volunteer.FirstName}!");
					return RedirectToAction("Details", new { volunteer.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Email"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, " +
						"you cannot have duplicate Email.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			return View(vVM);
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