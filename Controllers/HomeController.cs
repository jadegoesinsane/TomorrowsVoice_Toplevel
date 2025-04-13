using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NToastNotify;
using System.Diagnostics;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;
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

		public IActionResult Index(string? timeHours, string? timeShifts, string? dashaction)
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				if (User.IsInRole("Admin"))
				{
					if (dashaction == "timeHours")
					{
						HandleViewData("timeHours", timeHours);
					}
					else if (dashaction == "timeShifts")
					{
						HandleViewData("timeShifts", timeShifts);
					}
					else if (string.IsNullOrEmpty(ViewData["timeHours"]?.ToString()) && string.IsNullOrEmpty(ViewData["timeShifts"]?.ToString()))
					{
						HandleViewData("timeHours", timeHours);
						HandleViewData("timeShifts", timeShifts);
					}

					return View("Index", GetAdminHome(timeHours, timeShifts));
				}
				else if (User.IsInRole("Volunteer"))
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
				else if (!User.IsInRole("Admin"))
				{
					return RedirectToAction("Create", "VolunteerAccount");
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

		private HomeAdminVM GetAdminHome(string? timeHours, string? timeShifts)
		{
			string[] timeOptions = new[] { "This Month", "This Year", "Today" };

			var month = DateTime.Today.Month;
			var year = DateTime.Today.Year;
			var today = DateTime.Today;

			var singers = _context.Singers.AsNoTracking().ToList();

			var shifts = _context.Volunteers
				.Include(v => v.UserShifts)
					.ThenInclude(us => us.Shift)
				.SelectMany(v => v.UserShifts);
			IQueryable<UserShift>? shifts_fromlast;

			IQueryable<Shift>? shiftsFilled = _context.Shifts.Include(s => s.UserShifts);
			IQueryable<Shift>? shiftsFilled_fromlast;

			switch (ViewData["timeHours"]?.ToString())
			{
				case "This Month":
					shifts_fromlast = shifts.Where(us => us.Shift.ShiftDate.Month == month - 1 && us.Shift.ShiftDate.Year == year);
					shifts = shifts.Where(us => us.Shift.ShiftDate.Month == month && us.Shift.ShiftDate.Year == year);
					break;

				case "This Year":
					shifts_fromlast = shifts.Where(us => us.Shift.ShiftDate.Year == year - 1);
					shifts = shifts.Where(us => us.Shift.ShiftDate.Year == year);
					break;

				default:
					shifts_fromlast = shifts.Where(us => us.Shift.ShiftDate.Date == today.AddDays(-1).Date);
					shifts = shifts.Where(us => us.Shift.ShiftDate.Date == today.Date);
					break;
			}

			switch (ViewData["timeShifts"]?.ToString())
			{
				case "This Month":
					shiftsFilled_fromlast = shiftsFilled.Where(s => s.ShiftDate.Month == month - 1 && s.ShiftDate.Year == year);
					shiftsFilled = shiftsFilled.Where(s => s.ShiftDate.Month == month && s.ShiftDate.Year == year);
					break;

				case "This Year":
					shiftsFilled_fromlast = shiftsFilled.Where(s => s.ShiftDate.Year == year - 1);
					shiftsFilled = shiftsFilled.Where(s => s.ShiftDate.Year == year);
					break;

				default:
					shiftsFilled_fromlast = shiftsFilled.Where(s => s.ShiftDate.Date == today.AddDays(-1).Date);
					shiftsFilled = shiftsFilled.Where(s => s.ShiftDate.Date == today.Date);
					break;
			}

			var totalCurrentHours = TimeSpan.FromTicks(shifts.Sum(us => us.EndAt.Ticks - us.StartAt.Ticks)).TotalHours;
			var totalLastHours = TimeSpan.FromTicks(shifts_fromlast.Sum(us => us.EndAt.Ticks - us.StartAt.Ticks)).TotalHours;

			var adminHome = new HomeAdminVM
			{
				VolunteerHours = (int)totalCurrentHours,
				VolunteerHours_Change = (int)GetPercent(totalCurrentHours, totalLastHours),
				ShiftsFilled = (int)(shiftsFilled?.Sum(s => s.UserShifts.Count()) ?? 0),
				ShiftsFilled_Change = (int)GetPercent(shiftsFilled?.Sum(s => s.UserShifts.Count()) ?? 0, shiftsFilled_fromlast?.Sum(s => s.UserShifts.Count()) ?? 0),
				Transactions = GetTransactions()
			};
			return adminHome;
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

			if (volunteer == null)
				return new HomeVolunteerVM();

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

		#region Recent Activities

		public List<TransactionVM> GetTransactions()
		{
			var currentYear = DateTime.UtcNow.Year;

			var recentTransactions = _context.Transactions
				.Where(t => t.ChangeTimestamp.Year == currentYear)
				.Select(t => new
				{
					t.ID,
					t.ItemID,
					t.ChangedBy,
					t.ChangeType,
					t.ChangeTimestamp,
					t.OldValue,
					t.NewValue,
					t.Property,
					t.ItemType
				})
				.AsEnumerable()
				.SelectMany(t =>
				{
					var transactions = new List<TransactionVM>();

					var creator = GetCreator(t.ChangedBy);

					transactions.Add(new TransactionVM
					{
						ID = t.ID,
						ItemID = t.ItemID,
						ChangedBy = creator.Name,
						ChangedByID = creator.ID,
						ChangedByType = creator.Type,
						ChangeType = t.ChangeType,
						ChangeTimestamp = t.ChangeTimestamp,
						OldValue = t.OldValue,
						NewValue = t.NewValue,
						Property = t.Property,
						ItemType = t.ItemType,
						Description = GetDescription(t.ItemType, t.ItemID)
					});
					return transactions;
				})
				.OrderByDescending(t => t.ChangeTimestamp)
				.ToList();

			return recentTransactions;
		}

		public dynamic GetCreator(string email)
		{
			var director = _context.Directors.AsNoTracking().FirstOrDefault(d => d.Email == email);
			if (director != null)
				return new { Name = director.NameFormatted, ID = (int?)director.ID, Type = "Director" };

			var volunteer = _context.Volunteers.AsNoTracking().FirstOrDefault(v => v.Email == email);
			if (volunteer != null)
				return new { Name = volunteer.NameFormatted, ID = (int?)volunteer.ID, Type = "Volunteer" };

			if (email == "Seed Data")
				return new { Name = "Seed Data", ID = (int?)null, Type = "" };

			return new { Name = "Admin", ID = (int?)null, Type = "" };
		}

		public string GetDescription(string type, long id)
		{
			switch (type)
			{
				case "Singer":
					return $"{type} {_context.Singers.AsNoTracking().FirstOrDefault(s => s.ID == id)?.NameFormatted ?? "Unknown Singer"}";

				case "Director":
					return $"{type} {_context.Directors.AsNoTracking().FirstOrDefault(d => d.ID == id)?.NameFormatted ?? "Unknown Director"}";

				case "Rehearsal":
					return $"{type} on {_context.Rehearsals.AsNoTracking().FirstOrDefault(r => r.ID == id)?.Summary ?? "Unknown Rehearsal"}";

				case "Volunteer":
					return type + " " + _context.Volunteers.AsNoTracking().FirstOrDefault(v => v.ID == id)?.NameFormatted ?? "Unknown Volunteer";

				case "Event":
					return type + " " + _context.Events.AsNoTracking().FirstOrDefault(e => e.ID == id)?.Name ?? "Unknown Event";

				case "Chapter":
					return type + " " + _context.Chapters.AsNoTracking().Include(c => c.City).FirstOrDefault(c => c.ID == id)?.City.Name ?? "Unknown Chapter";

				default:
					return type;
			}
		}

		#endregion Recent Activities

		private void HandleViewData(string key, string? value)
		{
			string[] options = new[] { "This Month", "This Year", "Today" };
			if (!String.IsNullOrEmpty(value))
			{
				ViewData[key] = AssignNextOption(value, options);
			}
			else
			{
				ViewData[key] = options[0];
			}
		}

		private string AssignNextOption(string currentOption, string[] options)
		{
			int index = Array.IndexOf(options, currentOption);
			if (index == -1 || index == options.Length - 1)
				return options[0];
			else
				return options[index + 1];
		}

		private double GetPercent(double val1, double val2)
		{
			double percentChange;
			if (val2 != 0)
				percentChange = ((val1 - val2) / val2) * 100;
			else
				percentChange = val1 > 0 ? 100 : 0;
			return percentChange;
		}
	}
}