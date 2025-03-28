using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using NToastNotify.Helpers;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin, Planner, Volunteer")]
	public class EventController : ElephantController
	{
		private readonly TVContext _context;

		public EventController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> Index(string? SearchString, int? CityID, DateTime FilterStartDate,
			DateTime FilterEndDate, int? page, int? pageSizeID, string? actionButton, string? StatusFilter)
		{
			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;
			Enum.TryParse(StatusFilter, out Status selectedStatus);

			PopulateDropDownLists();
			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Closed)
						 .ToList();

			ViewBag.StatusList = new SelectList(statusList);

			await AutoClose();
			var events = _context.Events
				.Include(e => e.Colour)
				.Include(e => e.Shifts)
					.ThenInclude(s => s.UserShifts)
				.Include(e => e.Shifts)
					.ThenInclude(s => s.Colour)
				.AsNoTracking();

			if (!String.IsNullOrEmpty(StatusFilter))
			{
				events = events.Where(p => p.Status == selectedStatus);

				// filter out archived events if the user does not specifically select "archived"
				if (selectedStatus != Status.Closed)
				{
					events = events.Where(s => s.Status != Status.Closed);
				}
				numberFilters++;
			}
			// filter out events even if status filter has not been set
			else
			{
				events = events.Where(s => s.Status != Status.Closed);
			}
			//Filter For Start and End times
			if (FilterStartDate != default(DateTime) || FilterEndDate != default(DateTime))
			{
				if (FilterStartDate != default(DateTime))
				{
					ViewData["StartDate"] = FilterStartDate.ToString("yyyy-MM-dd");
				}
				if (FilterEndDate != default(DateTime))
				{
					ViewData["EndDate"] = FilterEndDate.ToString("yyyy-MM-dd");
				}
				events = events.Where(e => e.Shifts.Any(s =>
	(s.ShiftDate >= FilterStartDate) &&
	   s.ShiftDate <= FilterEndDate)
  );
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				events = events.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper()));
				numberFilters++;
			}
			if (CityID.HasValue)
			{
				events = events.Where(p => p.CityEvents.Any(ce => ce.CityID == CityID));

				numberFilters++;
			}
			//Give feedback about the state of the filters
			if (numberFilters != 0)
			{
				//Toggle the Open/Closed state of the collapse depending on if we are filtering
				ViewData["Filtering"] = " btn-danger";
				//Show how many filters have been applied
				ViewData["numberFilters"] = "(" + numberFilters.ToString()
					+ " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
				//Keep the Bootstrap collapse open
				@ViewData["ShowFilter"] = " show";
			}
			//Before we sort, see if we have called for a change of filtering or sorting
			if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
			{
				page = 1;//Reset page to start
			}

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);

			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		[Authorize(Roles = "Admin, Planner")]
		public IActionResult Create()
		{
			Event @event = new Event();
			//PopulateAssignedEnrollmentData(@event);
			PopulateDropDownLists(@event);
			return View();
		}

		// POST: Event/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Create([Bind("Name,StartDate,EndDate,Descripion,Location,CityEvents,ColourID")] Event @event, string[] selectedOptions, string Shifts)
		{
			try
			{
				//UpdateEnrollments(selectedOptions, @event);
				if (ModelState.IsValid)
				{
					var shifts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShiftJson>>(Shifts);
					foreach (var shiftJson in shifts)
					{
						@event.Shifts.Add(new Shift
						{
							Title = shiftJson.Title,
							ShiftDate = DateTime.Parse(shiftJson.Start).Date,
							StartAt = DateTime.Parse(shiftJson.Start),
							EndAt = DateTime.Parse(shiftJson.End),
							//BackgroundColor = string.IsNullOrEmpty(shiftJson.BackgroundColor) ? ColourPalette.BrightColours["Blue"] : shiftJson.BackgroundColor,
							Note = shiftJson.ExtendedProps?.Note,
							VolunteersNeeded = shiftJson.ExtendedProps?.VolunteersNeeded ?? 0
						});
					}
					string[]? cities = ModelState.GetValueOrDefault("CityEvents")?.AttemptedValue?.Split(',');
					if (cities != null)
					{
						foreach (string city in cities)
						{
							int.TryParse(city, out int id);
							@event.CityEvents.Add(new CityEvent { CityID = id, EventID = @event.ID });
						}
					}
					_context.Add(@event);
					await _context.SaveChangesAsync();
					Debug.WriteLine(@event.CityEvents.ToString());
					foreach (Shift shift in @event.Shifts)
					{
						shift.EventID = @event.ID;
					}
					AddSuccessToast(@event.Name);
					//_toastNotification.AddSuccessToastMessage($"{event.NameFormatted} was successfully created.");
					return RedirectToAction("Details", new { @event.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("@event.????????"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, " +
						"you cannot have duplicate Name and Email.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			PopulateAssignedEnrollmentData(@event);
			return View(@event);
		}

		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
			  .Include(g => g.CityEvents).ThenInclude(e => e.City)
			  .Include(e => e.Shifts)
			  .FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}
			ViewBag.ID = @event.ID;
			PopulateDropDownLists(@event);
			//PopulateAssignedEnrollmentData(@event);
			return View(@event);
		}

		// POST: Event/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Edit(int id, string[] CityEvents, string Shifts)
		{
			var @eventToUpdate = await _context.Events
			  .Include(g => g.CityEvents).ThenInclude(e => e.City)
			  .Include(e => e.Shifts)
			  .FirstOrDefaultAsync(m => m.ID == id);

			if (@eventToUpdate == null)
			{
				return NotFound();
			}
			ViewBag.ID = @eventToUpdate.ID;

			if (CityEvents != null)
			{
				eventToUpdate.CityEvents.Clear();
				foreach (string city in CityEvents)
				{
					int.TryParse(city, out int cID);
					@eventToUpdate.CityEvents.Add(new CityEvent { CityID = cID, EventID = @eventToUpdate.ID });
				}
			}
			//UpdateEnrollments(selectedOptions, @eventToUpdate);

			try
			{
				var shifts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShiftJson>>(Shifts);
				if (shifts != null)
				{
					eventToUpdate.Shifts.Clear();
					foreach (var shiftJson in shifts)
					{
						eventToUpdate.Shifts.Add(new Shift
						{
							Title = shiftJson.Title,
							ShiftDate = DateTime.Parse(shiftJson.Start).Date,
							StartAt = DateTime.Parse(shiftJson.Start),
							EndAt = DateTime.Parse(shiftJson.End),
							//BackgroundColor = string.IsNullOrEmpty(shiftJson.BackgroundColor) ? ColourPalette.BrightColours["Blue"] : shiftJson.BackgroundColor,
							Note = shiftJson.ExtendedProps?.Note,
							VolunteersNeeded = shiftJson.ExtendedProps?.VolunteersNeeded ?? 0
						});
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.GetBaseException().Message);
			}

			// Try updating with posted values
			if (await TryUpdateModelAsync<Event>(@eventToUpdate,
					"",
					   r => r.Name,
					   r => r.StartDate,
					 r => r.EndDate,
					 r => r.Location,
					r => r.Descripion,
					r => r.Status,
					r => r.ColourID))
			{
				try
				{
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{@eventToUpdate.Name} was successfully updated.");
					return RedirectToAction("Details", new { @eventToUpdate.ID });
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
			PopulateAssignedEnrollmentData(@eventToUpdate);

			return View(@eventToUpdate);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		// POST: Event/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var @event = await _context.Events.Include(c => c.Shifts)

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (@event != null)
				{
					_context.Events.Remove(@event);
					/*foreach (var a in @event.Shifts)
					{
						a.Status = Status.Archived;
					}
					// Here we are archiving a event instead of deleting them
					@event.Status = Status.Archived;*/

					await _context.SaveChangesAsync();
					AddSuccessToast(@event.Name);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(@event);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Close(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		// POST: Event/Delete/5
		[HttpPost, ActionName("Close")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CloseConfirmed(int id)
		{
			var @event = await _context.Events.Include(c => c.Shifts)

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (@event != null)
				{
					// _context.Events.Remove(@event);
					foreach (var a in @event.Shifts)
					{
						a.Status = Status.Closed;
					}
					// Here we are archiving a event instead of deleting them
					@event.Status = Status.Closed;

					await _context.SaveChangesAsync();
					AddSuccessToast(@event.Name);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(@event);
		}

		public async Task<IActionResult> AutoClose()
		{
			var today = DateTime.Now;

			var events = _context.Events
				.Include(e => e.Shifts)
				.Where(e => e.EndDate < today)
				.ToList();

			foreach (var e in events)
			{
				e.Status = Status.Closed;

				foreach (var s in e.Shifts)
				{
					s.Status = Status.Closed; 
				}
			}

			await _context.SaveChangesAsync();
			return View(events);
		}


		[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Recover(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		// POST: Event/Recover/5
		[HttpPost, ActionName("Recover")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RecoverConfirmed(int id)
		{
			var @event = await _context.Events.Include(c => c.Shifts)

			 .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (@event != null)
				{
					//_context.Events.Remove(event);
					foreach (var a in @event.Shifts)
					{
						a.Status = Status.Active;
					}
					// Here we are archiving a event instead of deleting them
					@event.Status = Status.Active;
					await _context.SaveChangesAsync();
					AddSuccessToast(@event.Name);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(@event);
		}

		public async Task<IActionResult> EventCalendar()
		{
			var events = _context.Events
				.Include(e => e.Shifts)
				.ThenInclude(s => s.UserShifts)
				.AsNoTracking();

			return View(events);
		}

		//[HttpGet]
		//public async Task<IActionResult> GetShifts(int id)
		//{
		//	var shifts = await _context.Shifts
		//		.Where(s => s.EventID == id)
		//		.ToListAsync();

		//	return Json(shifts);
		//}

		#region Full Calendars

		public JsonResult GetShifts(int id)
		{
			var shifts = _context.Shifts
				.Where(s => s.EventID == id)
				.Include(s => s.Colour)
				.Select(s => new
				{
					Title = s.Title ?? string.Empty,
					Start = s.StartAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					End = s.EndAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					BackgroundColor = s.Colour.BackgroundColour,
					BorderColor = s.Colour.BorderColour,
					TextColor = s.Colour.TextColour,
					Display = "block",
					displayEventEnd = true,
					ExtendedProps = new ExtendedProps
					{
						VolunteersNeeded = s.VolunteersNeeded,
						Note = s.Note ?? string.Empty,
						Location = s.Location ?? string.Empty
					}
				}).ToList();

			return Json(shifts);
		}

		public JsonResult GetEvents()
		{
			var events = _context.Events
				.Include(e => e.Colour)
				.Select(e => new
				{
					ID = e.ID,
					Title = e.Name ?? string.Empty,
					Start = e.StartDate.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					End = e.EndDate.AddDays(1).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					BackgroundColor = e.Colour.BackgroundColour,
					BorderColor = e.Colour.BorderColour,
					TextColor = e.Colour.TextColour,
					allDay = true,
					isShift = false
				}).ToList();
			return Json(events);
		}

		public JsonResult GetEventShifts()
		{
			var shifts = _context.Shifts
				.Include(s => s.Colour)
				.Select(s => new
				{
					ID = s.ID,
					Title = s.Title ?? "Shift",
					Start = s.ShiftDate.AddHours(s.StartAt.Hour).AddMinutes(s.StartAt.Minute).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					End = s.ShiftDate.AddHours(s.EndAt.Hour).AddMinutes(s.EndAt.Minute).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
					BackgroundColor = s.Colour.BackgroundColour,
					BorderColor = s.Colour.BorderColour,
					TextColor = s.Colour.TextColour,
					Display = "list-item",
					displayEventEnd = true,
					isShift = true,
					ExtendedProps = new ExtendedProps
					{
						VolunteersNeeded = s.VolunteersNeeded,
						Note = s.Note ?? string.Empty,
						Location = s.Location ?? string.Empty
					}
				}).ToList();

			return Json(shifts);
		}

		#endregion Full Calendars

		private void PopulateAssignedEnrollmentData(Event abc)
		{
			//For this to work, you must have Included the child collection in the parent object
			var allOptions = _context.Cities;
			var currentOptionsHS = new HashSet<int>(abc.CityEvents.Select(b => b.CityID));
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
						DisplayText = c.Name
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.Name
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}

		private void UpdateEnrollments(string[] selectedOptions, Event abcToUpdate)
		{
			if (selectedOptions == null)
			{
				abcToUpdate.CityEvents = new List<CityEvent>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(abcToUpdate.CityEvents.Select(b => b.CityID));
			foreach (var c in _context.Volunteers)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						abcToUpdate.CityEvents.Add(new CityEvent
						{
							CityID = c.ID,
							EventID = abcToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						CityEvent? enrollmentToRemove = abcToUpdate.CityEvents
							.FirstOrDefault(d => d.CityID == c.ID);
						if (enrollmentToRemove != null)
						{
							_context.Remove(enrollmentToRemove);
						}
					}
				}
			}
		}

		private void PopulateDropDownLists(Event? events = null)
		{
			ViewData["CityEvents"] = CitySelectList(events?.CityEvents.Select(c => c.CityID).ToArray(), Status.Active);
			ViewData["BackgroundColour"] = ColourSelectList(events?.ColourID);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Inactive)
						 .ToList();

			ViewBag.CityOptions = CitySelectList(events?.CityEvents.Select(c => c.CityID).ToArray(), Status.Active);

			ViewBag.StatusList = new SelectList(statusList);
		}

		public JsonResult GetEventData()
		{
			var data = new Event
			{
				Name = "Sas Gift Wrapping - 2025",
				StartDate = new DateTime(2025, 11, 30),
				EndDate = new DateTime(2025, 12, 22),
				Location = "Midtown, Saskatoon"
			};
			return Json(data);
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> IndexVolunteer(int? id, string? SearchString, string? Location, DateTime FilterStartDate,
			DateTime FilterEndDate, int? page, int? pageSizeID, string? actionButton, string? StatusFilter)
		{
			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;
			Enum.TryParse(StatusFilter, out Status selectedStatus);

			PopulateDropDownLists();
			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Archived)
						 .ToList();

			ViewBag.StatusList = new SelectList(statusList);
			ViewData["VolunteerID"] = id;
			var events = _context.Events
				.Include(e => e.Shifts)
				.ThenInclude(s => s.UserShifts)
				.AsNoTracking();

			if (!String.IsNullOrEmpty(StatusFilter))
			{
				events = events.Where(p => p.Status == selectedStatus);

				// filter out archived events if the user does not specifically select "archived"
				if (selectedStatus != Status.Archived)
				{
					events = events.Where(s => s.Status != Status.Archived);
				}
				numberFilters++;
			}
			// filter out events even if status filter has not been set
			else
			{
				events = events.Where(s => s.Status != Status.Archived);
			}
			//Filter For Start and End times
			if (FilterStartDate != default(DateTime) || FilterEndDate != default(DateTime))
			{
				if (FilterStartDate != default(DateTime))
				{
					ViewData["StartDate"] = FilterStartDate.ToString("yyyy-MM-dd");
				}
				if (FilterEndDate != default(DateTime))
				{
					ViewData["EndDate"] = FilterEndDate.ToString("yyyy-MM-dd");
				}
				events = events.Where(e => e.StartDate >= FilterStartDate && e.EndDate <= FilterEndDate);
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				events = events.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper()));
				numberFilters++;
			}
			if (!String.IsNullOrEmpty(Location))
			{
				events = events.Where(e => e.Location.Contains(Location));
				numberFilters++;
			}
			//Give feedback about the state of the filters
			if (numberFilters != 0)
			{
				//Toggle the Open/Closed state of the collapse depending on if we are filtering
				ViewData["Filtering"] = " btn-danger";
				//Show how many filters have been applied
				ViewData["numberFilters"] = "(" + numberFilters.ToString()
					+ " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
				//Keep the Bootstrap collapse open
				@ViewData["ShowFilter"] = " show";
			}
			//Before we sort, see if we have called for a change of filtering or sorting
			if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
			{
				page = 1;//Reset page to start
			}

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		public IActionResult DownloadEvents(int? id, string? name)
		{
			// Get Event Details
			var events = from e in _context.Events
						 .Where(e => e.ID == id)
						 select new
						 {
							 Date = e.DateSummary,
							 Location = e.Location,
							 Description = e.Descripion
						 };

			// Get Shift Details
			var shifts = from s in _context.Shifts
						 .Include(s => s.UserShifts)
						 .Where(s => s.EventID == id)
						 select new
						 {
							 Date = s.ShiftDate.ToShortDateString(),
							 Times = s.TimeFormat,
							 Volunteers = $"{s.UserShifts.Count()} / {s.VolunteersNeeded}"
						 };

			// How many rows?
			int eventNumRows = events.Count();
			int shiftNumRows = shifts.Count();

			if (eventNumRows > 0) // We have data
			{
				// Create a new spreadsheet from scratch.
				using (ExcelPackage excel = new ExcelPackage())
				{
					var workSheet = excel.Workbook.Worksheets.Add("Events");

					// Note: Cells[row, column]
					workSheet.Cells[3, 1].LoadFromCollection(events, true);
					workSheet.Cells[9, 1].LoadFromCollection(shifts, true);

					// Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
					workSheet.Cells[4, 1, eventNumRows + 3, 3].Style.Font.Bold = true;
					workSheet.Cells[10, 1, shiftNumRows + 10, 3].Style.Font.Bold = true;

					// Set Style and background color of headings
					// Event Headings
					using (ExcelRange headings = workSheet.Cells[3, 1, 3, 3])
					{
						headings.Style.Font.Bold = true;
						var fill = headings.Style.Fill;
						fill.PatternType = ExcelFillStyle.Solid;
						fill.BackgroundColor.SetColor(Color.LightBlue);
					}
					// Shift Headings
					using (ExcelRange headings = workSheet.Cells[9, 1, 9, 3])
					{
						headings.Style.Font.Bold = true;
						var fill = headings.Style.Fill;
						fill.PatternType = ExcelFillStyle.Solid;
						fill.BackgroundColor.SetColor(Color.LightGreen);
					}

					// Autofit columns
					workSheet.Cells.AutoFitColumns();

					// Add a title and timestamp at the top of the report
					// Event Title
					workSheet.Cells[1, 1].Value = $"{name} Event Report";
					using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 3])
					{
						Rng.Merge = true; // Merge columns start and end range
						Rng.Style.Font.Bold = true; // Font should be bold
						Rng.Style.Font.Size = 18;
						Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}
					// Shift Title
					workSheet.Cells[8, 1].Value = "Shift Report";
					using (ExcelRange Rng = workSheet.Cells[8, 1, 8, 3])
					{
						Rng.Merge = true; // Merge columns start and end range
						Rng.Style.Font.Bold = true; // Font should be bold
						Rng.Style.Font.Size = 14;
						Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}

					// Since the time zone where the server is running can be different, adjust to
					// Local for us.
					DateTime utcDate = DateTime.UtcNow;
					TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
					DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
					using (ExcelRange Rng = workSheet.Cells[2, 3])
					{
						Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
							localDate.ToShortDateString();
						Rng.Style.Font.Bold = true; // Font should be bold
						Rng.Style.Font.Size = 12;
						Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
					}

					// Ok, time to download the Excel
					try
					{
						Byte[] theData = excel.GetAsByteArray();
						string filename = $"{name} Report.xlsx";
						string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
						return File(theData, mimeType, filename);
					}
					catch (Exception)
					{
						return BadRequest("Could not build and download the file.");
					}
				}
			}
			return NotFound("No data.");
		}

		//private SelectList CitySelectList()
		//{
		//	return new SelectList(_context
		//		.Cities
		//		.OrderBy(c => c.Name), "ID", "Name");
		//}

		public JsonResult AddShift(string? thing)
		{
			return Json(new { success = true, message = "Shift added successfully" });
		}

		private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.ID == id);
		}
	}
}