using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin,Planner,Volunteer")]
	public class VolunteerShiftController : ElephantController
	{
		private readonly TVContext _context;

		public VolunteerShiftController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int? VolunteerID)
		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");

			Volunteer? volunteer = null;
			if (User.IsInRole("Volunteer") || User.IsInRole("Planner"))
			{
				volunteer = await _context.Volunteers
				.Where(v => v.Email == User.Identity.Name)
				.AsNoTracking()
				.FirstOrDefaultAsync();
				VolunteerID = volunteer?.ID ?? null;
			}

			if (!VolunteerID.HasValue || volunteer == null)
			{
				return Redirect(ViewData["returnURL"].ToString());
			}
			var shifts = await _context.Shifts
				.Include(s => s.UserShifts)
					.ThenInclude(us => us.User)
				.Include(s => s.Event)
				.Where(s => s.UserShifts.Any(us => us.UserID == VolunteerID.GetValueOrDefault()))
				.OrderByDescending(s => s.StartAt)
				.Where(s => s.Status == Status.Active)
				.AsNoTracking()
				.ToListAsync();

			ViewBag.Volunteer = volunteer;

			return View(shifts);
		}

		public async Task<IActionResult> SignUp(int? ShiftID)
		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

			Volunteer? volunteer = await GetVolunteerFromUser();

			if (volunteer == null)
				return Redirect(ViewData["returnURL"].ToString());

			if (_context.UserShifts.Any(us => us.UserID == volunteer.ID && us.ShiftID == ShiftID))
			{
				_toastNotification.AddErrorToastMessage("You are already signed up for this shift!");
				return Redirect(ViewData["returnURL"].ToString());
			}

			var userShift = new UserShift
			{
				UserID = volunteer.ID,
				ShiftID = (int)ShiftID
			};

			_context.Add(userShift);
			await _context.SaveChangesAsync();
			_toastNotification.AddSuccessToastMessage("Signed up for shift!");

			return Redirect(ViewData["returnURL"].ToString());
		}

		private async Task<Volunteer?> GetVolunteerFromUser()
		{
			return await _context.Volunteers
				.Where(v => v.Email == User.Identity.Name)
				.AsNoTracking()
				.FirstOrDefaultAsync();
		}
	}
}