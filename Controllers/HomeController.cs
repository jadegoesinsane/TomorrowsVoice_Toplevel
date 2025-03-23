using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NToastNotify;
using System.Diagnostics;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly TVContext _context;

		public HomeController(ILogger<HomeController> logger, TVContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
			if (User.Identity.IsAuthenticated)
			{
				if (User.IsInRole("Volunteer"))
				{
					var model = GetVolunteerHome();
					return View("Index", model);
				}
				else if (User.IsInRole("Director"))
				{
					var model = GetDirectorHome();
					return View("Index", model);
				}
				else if (User.IsInRole("Planner"))
				{
					var model = GetPlannerHome();
					return View("Index", model);
				}
			}
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private HomeDirectorVM GetDirectorHome()
		{
			var dID = _context.Directors
				.Where(d => d.Email == User.Identity.Name.ToString())
				.FirstOrDefault();

			var activeSingers = _context.Singers.Where(s => s.Status == Status.Active && s.ChapterID == dID.ChapterID);
			var activeRehearsal = _context.Rehearsals.Where(r => r.Status == Status.Active && r.ChapterID == dID.ChapterID);

			return new HomeDirectorVM
			{
				SingerCount = activeSingers.Count(),
				RehearsalCount = activeRehearsal.Count(),
				DirectorCount = _context.Directors.Where(s => s.Status == Status.Active).Count(),
				ChapterCount = _context.Chapters.Where(c => c.Status == Status.Active).Count()
			};
		}

		private HomePlannerVM GetPlannerHome()
		{
			return new HomePlannerVM
			{
				EventCount = _context.Events.Where(c => c.Status == Status.Active).Count(),
				VolunteerCount = _context.Volunteers.Where(c => c.Status == Status.Active).Count(),
				ShiftCount = _context.Shifts.Where(c => c.Status == Status.Active).Count(),
				CityCount = _context.Cities.Count()
			};
		}

		public HomeVolunteerVM GetVolunteerHome()
		{
			var volunteer = _context.Volunteers
					.Where(v => v.Email == User.Identity.Name.ToString())
					.Include(v => v.UserShifts)
					.ThenInclude(us => us.Shift)
					.AsNoTracking()
					.FirstOrDefault();

			decimal progress = 0;
			if (volunteer.YearlyVolunteerGoal.HasValue && volunteer.YearlyVolunteerGoal > 0)
			{
				progress = (decimal)volunteer.HoursVolunteered / volunteer.YearlyVolunteerGoal.Value * 100;
			}
			return new HomeVolunteerVM
			{
				Name = volunteer.NameFormatted,
				HourlyGoal = volunteer.YearlyVolunteerGoal ?? 0,
				TimeWorked = volunteer.TotalWorkDuration,
				Progress = (int)progress,
				Shifts = _context.Shifts
				.Where(s => s.UserShifts.Any(us => us.UserID == volunteer.ID) && s.Status == Status.Active)
				.Include(s => s.Event)
				.AsNoTracking()
				.ToList()
			};
		}
	}
}