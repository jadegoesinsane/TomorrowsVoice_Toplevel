using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class EventController : ElephantController
	{
		private readonly TVContext _context;

		public EventController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: Event
		public async Task<IActionResult> Index(string? SearchString, string? Location, DateTime FilterStartDate,
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

		// GET: Event/Details/5
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

		// GET: Event/Create
		public IActionResult Create()
		{
			Event @event = new Event();
			PopulateAssignedEnrollmentData(@event);
			return View();
		}

		// POST: Event/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,StartDate,EndDate,Descripion,Location,Shifts")] Event @event, string[] selectedOptions)
		{
			try
			{
				UpdateEnrollments(selectedOptions, @event);
				if (ModelState.IsValid)
				{
					_context.Add(@event);
					await _context.SaveChangesAsync();
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

		// GET: Event/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
			  .Include(g => g.CityEvents).ThenInclude(e => e.City)
			  .FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			PopulateAssignedEnrollmentData(@event);
			return View(@event);
		}

		// POST: Event/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions)
		{
			var @eventToUpdate = await _context.Events
			  .Include(g => g.CityEvents).ThenInclude(e => e.City)
			  .FirstOrDefaultAsync(m => m.ID == id);

			if (@eventToUpdate == null)
			{
				return NotFound();
			}
			UpdateEnrollments(selectedOptions, @eventToUpdate);
			// Try updating with posted values
			if (await TryUpdateModelAsync<Event>(@eventToUpdate,
					"",
					   r => r.Name,
					   r => r.StartDate,
					 r => r.EndDate,
					 r => r.Location,
					r => r.Descripion,
					r => r.Status))
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

		// GET: Event/Delete/5
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
		public async Task<IActionResult> DeleteConfirmed(int id)
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
						a.Status = Status.Archived;
					}
					// Here we are archiving a event instead of deleting them
					@event.Status = Status.Archived;

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

		public async Task<IActionResult> GetShifts()
		{
			var shifts = await _context.Shifts
				.Include(s => s.Event)
				.Select(s => new
				{
					id = s.ID,
					groupId = s.EventID,
					title = $"{s.TimeFormat}",
					start = $"{s.ShiftDate.ToString("yyyy-MM-dd")}{s.StartAt.ToString("THH:mm:ss")}",
					end = $"{s.ShiftDate.ToString("yyyy-MM-dd")}{s.EndAt.ToString("THH:mm:ss")}",
					display = "list-item",
					borderColor =
					s.EventID == 1 ? "#bc3700" : // light red
					s.EventID == 2 ? "#cc351e" : // dark red
					s.EventID == 3 ? "#00863e" : // light green
					s.EventID == 4 ? "#005d1f" : // dark green
					s.EventID == 5 ? "#856288" : // light purple
					s.EventID == 6 ? "#523256" : // dark purple
					s.EventID == 7 ? "#0176ad" : // light blue
					s.EventID == 8 ? "#035083" : // dark blue
					s.EventID == 9 ? "#222bff" : // really bright blue
					s.EventID == 10 ? "#8325f2" : // really bright purple
					s.EventID == 11 ? "#cc326d" : // dark pink
					s.EventID == 12 ? "#a15c80" : // light pink
					"#fff",
					textColor =
					s.EventID == 1 ? "#bc3700" : // light red
					s.EventID == 2 ? "#cc351e" : // dark red
					s.EventID == 3 ? "#00863e" : // light green
					s.EventID == 4 ? "#005d1f" : // dark green
					s.EventID == 5 ? "#856288" : // light purple
					s.EventID == 6 ? "#523256" : // dark purple
					s.EventID == 7 ? "#0176ad" : // light blue
					s.EventID == 8 ? "#035083" : // dark blue
					s.EventID == 9 ? "#222bff" : // really bright blue
					s.EventID == 10 ? "#8325f2" : // really bright purple
					s.EventID == 11 ? "#cc326d" : // dark pink
					s.EventID == 12 ? "#a15c80" : // light pink
					"#fff",
					url = Url.Action("Details", "Shift", new { id = s.ID })
				})
				.ToListAsync();
			return Json(shifts);
		}

		public async Task<IActionResult> GetEvents()
		{
			var events = await _context.Events
				.Include(e => e.Shifts)
				.Select(e => new
				{
					id = e.ID,
					groupId = e.ID,
					title = e.Name,
					start = $"{e.StartDate.ToString("yyyy-MM-dd")}{e.StartDate.ToString("THH:mm:ss")}",
					end = $"{e.EndDate.AddDays(1).ToString("yyyy-MM-dd")}{e.EndDate.ToString("THH:mm:ss")}",
					display = "block",
					textColor = "white",
					borderColor =
					e.ID == 1 ? "#bc3700" : // light red
					e.ID == 2 ? "#cc351e" : // dark red
					e.ID == 3 ? "#00863e" : // light green
					e.ID == 4 ? "#005d1f" : // dark green
					e.ID == 5 ? "#856288" : // light purple
					e.ID == 6 ? "#523256" : // dark purple
					e.ID == 7 ? "#0176ad" : // light blue
					e.ID == 8 ? "#035083" : // dark blue
					e.ID == 9 ? "#222bff" : // really bright blue
					e.ID == 10 ? "#8325f2" : // really bright purple
					e.ID == 11 ? "#cc326d" : // dark pink
					e.ID == 12 ? "#a15c80" : // light pink
					"#fff",
					backgroundColor =
					e.ID == 1 ? "#bc3700" : // light red
					e.ID == 2 ? "#cc351e" : // dark red
					e.ID == 3 ? "#00863e" : // light green
					e.ID == 4 ? "#005d1f" : // dark green
					e.ID == 5 ? "#856288" : // light purple
					e.ID == 6 ? "#523256" : // dark purple
					e.ID == 7 ? "#0176ad" : // light blue
					e.ID == 8 ? "#035083" : // dark blue
					e.ID == 9 ? "#222bff" : // really bright blue
					e.ID == 10 ? "#8325f2" : // really bright purple
					e.ID == 11 ? "#cc326d" : // dark pink
					e.ID == 12 ? "#a15c80" : // light pink
					"#fff",
					url = Url.Action("Details", "Event", new { id = e.ID })
				})
				.ToListAsync();
			return Json(events);
		}

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
			var location = EventLocationSelectList();

			ViewData["Location"] = new SelectList(location, events?.Location);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Inactive)
						 .ToList();

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

		private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.ID == id);
		}
	}
}