using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NToastNotify;
using NToastNotify.Helpers;
using NuGet.Protocol;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SQLitePCL;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin, Planner, Volunteer")]
	public class ShiftController : ElephantController
	{
		private readonly TVContext _context;
		private readonly IMyEmailSender _emailSender;
		public ShiftController(TVContext context, IMyEmailSender emailSender, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_emailSender = emailSender;
			_context = context;
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> Index(int? EventID, string? Location, DateTime FilterStartDate,
			int? page, int? pageSizeID, string? actionButton, string? StatusFilter)

		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

			if (!EventID.HasValue)
			{
				//Go back to the proper return url for the Events controller
				return Redirect(ViewData["returnURL"].ToString());
			}

			//Count the number of filters applied - start by assuming no filters
			//ViewData["Filtering"] = "btn-outline-secondary";
			//int numberFilters = 0;
			//Enum.TryParse(StatusFilter, out Status selectedStatus);

			//PopulateDropDown2();

			var shifts = _context.Shifts
				.Include(s => s.Event)
				.Include(s => s.UserShifts)
				.Where(s => s.EventID == EventID.GetValueOrDefault())
				.AsNoTracking();

			//if (!String.IsNullOrEmpty(StatusFilter))
			//{
			//	shifts = shifts.Where(p => p.Status == selectedStatus);

			//	// filter out archived events if the user does not specifically select "archived"
			//	if (selectedStatus != Status.Archived)
			//	{
			//		shifts = shifts.Where(s => s.Status != Status.Archived);
			//	}
			//	numberFilters++;
			//}
			//// filter out events even if status filter has not been set
			//else
			//{
			//	shifts = shifts.Where(s => s.Status != Status.Archived);
			//}
			//Filter For Start and End times
			//if (FilterStartDate != default(DateTime))
			//{
			//	if (FilterStartDate != default(DateTime))
			//	{
			//		ViewData["StartDate"] = FilterStartDate.ToString("yyyy-MM-dd");
			//	}
			//	shifts = shifts.Where(e => e.ShiftDate == FilterStartDate);
			//}

			//Give feedback about the state of the filters
			//if (numberFilters != 0)
			//{
			//	//Toggle the Open/Closed state of the collapse depending on if we are filtering
			//	ViewData["Filtering"] = " btn-danger";
			//	//Show how many filters have been applied
			//	ViewData["numberFilters"] = "(" + numberFilters.ToString()
			//		+ " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
			//	//Keep the Bootstrap collapse open
			//	@ViewData["ShowFilter"] = " show";
			//}
			////Before we sort, see if we have called for a change of filtering or sorting
			//if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
			//{
			//	page = 1;//Reset page to start

			//}

			// MASTER Record, Events
			Event? events = await _context.Events.FirstOrDefaultAsync(e => e.ID == EventID.GetValueOrDefault());

			// Pass ID of Next and Previous Event into viewdata
			int currentEvent = events.ID;
            var eventList = _context.Events.OrderBy(e=>e.StartDate).Where(e=>e.Status == Status.Active).Select(e => e.ID).ToList(); // If event index sorting is changed, will have to change OrderBy clause here too
            int index = eventList.FindIndex(x=>x == currentEvent);
			// Check to see if we have reached either end of the event list
			try
			{
                ViewData["previousEvent"] = eventList[index - 1];
            }
			catch (Exception)
			{
				ViewData["previousEvent"] = "Invalid";
            }
			try
			{
                ViewData["nextEvent"] = eventList[index + 1];
            }
			catch (Exception)
			{
				ViewData["nextEvent"] = "Invalid";
            }
			
			
			
			
			ViewBag.Event = events;

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Shift>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
				.Include(s => s.Event)
				.Include(s => s.UserShifts)
					.ThenInclude(us => us.User)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (shift == null)
			{
				return NotFound();
			}

			// To show volunteer select list
			PopulateVolunteers();

			return View(shift);
		}

		[Authorize(Roles = "Admin, Planner")]
		public IActionResult Create(int EventID)
		{
			Shift shift = new Shift();

			ViewData["EventID"] = EventID;
			var eventData = _context.Events
			.Where(e => e.ID == EventID)
			.FirstOrDefault();

			ViewData["Event"] = $"{eventData.Name} ({eventData.StartDate:MM-dd-yyyy} - {eventData.EndDate:MM-dd-yyyy})";

			return View();
		}

		// POST: Shift/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Create([Bind("ID,ShiftDate,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift, string[] selectedOptions)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var eventRecord = _context.Events
									.FirstOrDefault(e => e.ID == shift.EventID);
					if (shift.ShiftDate < eventRecord.StartDate || shift.ShiftDate > eventRecord.EndDate)
					{
						// Throwing exception when overlap condition is met
						throw new DbUpdateException("Unable to save changes. Your date is out of Event range..");
					}

					var sameshifts = _context.Shifts
						.Where(r => r.ShiftDate == shift.ShiftDate && r.EventID == shift.EventID);

					if (sameshifts.Count() != 0)
					{
						foreach (var ABC in sameshifts)
						{
							if (ABC.ID != shift.ID) // Don't compare it to itself!
							{
								// Check if it Overlaps
								if (shift.StartAt.TimeOfDay < ABC.EndAt.TimeOfDay && shift.StartAt.TimeOfDay >= ABC.StartAt.TimeOfDay)
								{
									// Throwing exception when overlap condition is met
									throw new DbUpdateException("Unable to save changes. Remember, you cannot have overlapping shifts.");
								}
								else if (shift.EndAt.TimeOfDay <= ABC.EndAt.TimeOfDay && shift.EndAt.TimeOfDay > ABC.StartAt.TimeOfDay)
								{
									// Throwing exception when overlap condition is met
									throw new DbUpdateException("Unable to save changes. Remember, you cannot have overlapping shifts.");
								}
							}
						}
					}

					_context.Add(shift);
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{shift.Title ?? "Shift"} at {shift.TimeFormat} was successfully created.");
					// _toastNotification.AddSuccessToastMessage($"{singer.NameFormatted} was successfully created.");
					return RedirectToAction("Details", new { shift.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("overlapping shifts"))
				{
					// Handling the custom exception
					ModelState.AddModelError("", "Unable to save changes. Shifts overlap.");
				}
				else if (message.Contains("Event range"))
				{
					// Handling the custom exception
					ModelState.AddModelError("", "Unable to save changes. Your date is out of Event range..");
				}
				else if (message.Contains("UNIQUE") && message.Contains("volunteer.Email"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have duplicate Name and Email.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			ViewData["EventID"] = shift.EventID;
			var eventData = _context.Events
			.Where(e => e.ID == shift.EventID)
			.FirstOrDefault();

			ViewData["Event"] = $"{eventData.Name} ({eventData.StartDate:MM-dd-yyyy} - {eventData.EndDate:MM-dd-yyyy})";

			return View();
		}

		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
			   .Include(g => g.UserShifts).ThenInclude(e => e.User)
			   .FirstOrDefaultAsync(m => m.ID == id);
			if (shift == null)
			{
				return NotFound();
			}
			PopulateAssignedEnrollmentData(shift);
			PopulateDropDown(shift);
			return View(shift);
		}

		// POST: Shift/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions)
		{
			var shiftToUpdate = await _context.Shifts
			   .Include(g => g.UserShifts).ThenInclude(e => e.User)
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (shiftToUpdate == null)
			{
				return NotFound();
			}
			UpdateEnrollments(selectedOptions, shiftToUpdate);
			// Try updating with posted values
			if (await TryUpdateModelAsync<Shift>(shiftToUpdate,
					"",
					r => r.ShiftDate,
					r => r.StartAt,
					r => r.EndAt, r => r.Status,
				   r => r.VolunteersNeeded,
				   r => r.EventID
					))
			{
				try
				{
					var sameshift = await _context.Shifts
						 .Include(s => s.Event)
						 .FirstOrDefaultAsync(m => m.EventID == shiftToUpdate.EventID);

					if (shiftToUpdate.ShiftDate < sameshift.Event.StartDate || shiftToUpdate.ShiftDate > sameshift.Event.EndDate)
					{
						// Throwing exception when overlap condition is met
						throw new DbUpdateException("Unable to save changes. Your date is out of Event range..");
					}

					var sameshifts = _context.Shifts
						.Where(r => r.ShiftDate == shiftToUpdate.ShiftDate && r.EventID == shiftToUpdate.EventID);

					if (sameshifts.Count() != 0)
					{
						foreach (var ABC in sameshifts)
						{
							if (ABC.ID != shiftToUpdate.ID) // Don't compare it to itself!
							{
								// Check if it Overlaps
								if (shiftToUpdate.StartAt.TimeOfDay < ABC.EndAt.TimeOfDay && shiftToUpdate.StartAt.TimeOfDay > ABC.StartAt.TimeOfDay)
								{
									// Throwing exception when overlap condition is met
									throw new DbUpdateException("Unable to save changes. Remember, you cannot have overlapping shifts.");
								}
								else if (shiftToUpdate.EndAt.TimeOfDay < ABC.EndAt.TimeOfDay && shiftToUpdate.EndAt.TimeOfDay > ABC.StartAt.TimeOfDay)
								{
									// Throwing exception when overlap condition is met
									throw new DbUpdateException("Unable to save changes. Remember, you cannot have overlapping shifts.");
								}
							}
						}
					}
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{shiftToUpdate.Title ?? "Shift"} at {shiftToUpdate.TimeFormat} was successfully updated.");
					return RedirectToAction("Details", new { shiftToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("overlapping shifts"))
					{
						// Handling the custom exception
						ModelState.AddModelError("", "Unable to save changes. Shifts overlap.");
					}
					else if (message.Contains("Event range"))
					{
						// Handling the custom exception
						ModelState.AddModelError("", "Unable to save changes. Your date is out of Event range..");
					}
					else if (message.Contains("UNIQUE") && message.Contains("volunteer.Email"))
					{
						ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have duplicate Name and Email.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}
			PopulateAssignedEnrollmentData(shiftToUpdate);
			PopulateDropDown(shiftToUpdate);
			return View(shiftToUpdate);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
				.Include(s => s.Event)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (shift == null)
			{
				return NotFound();
			}

			return View(shift);
		}

		// POST: Shift/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var shift = await _context.Shifts
			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (shift != null)
				{
					_context.Shifts.Remove(shift);
					//shift.Status = Status.Archived;
					// Here we are archiving a singer instead of deleting them
					//_context.Shifts.Remove(shift);
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{shift.Title ?? "Shift"} at {shift.TimeFormat} was successfully deleted.");
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(shift);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Recover(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
				.Include(s => s.Event)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (shift == null)
			{
				return NotFound();
			}

			return View(shift);
		}

		// POST: Shift/Delete/5
		[HttpPost, ActionName("Recover")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RecoverConfirmed(int id)
		{
			var shift = await _context.Shifts

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (shift != null)
				{
					//_context.Singers.Remove(singer);
					shift.Status = Status.Active;
					// Here we are archiving a singer instead of deleting them
					//_context.Shifts.Remove(shift);
					await _context.SaveChangesAsync();
					_toastNotification.AddSuccessToastMessage($"{shift.Title ?? "Shift"} at {shift.TimeFormat} was successfully recovered.");
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(shift);
		}

		private void PopulateAssignedEnrollmentData(Shift shift)
		{
			//For this to work, you must have Included the child collection in the parent object
			var allOptions = _context.Volunteers;
			var currentOptionsHS = new HashSet<int>(shift.UserShifts.Select(b => b.UserID));
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
						DisplayText = c.NameFormatted
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.NameFormatted
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}

		private void UpdateEnrollments(string[] selectedOptions, Shift shiftToUpdate)
		{
			if (selectedOptions == null)
			{
				shiftToUpdate.UserShifts = new List<UserShift>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(shiftToUpdate.UserShifts.Select(b => b.UserID));
			foreach (var c in _context.Volunteers)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						shiftToUpdate.UserShifts.Add(new UserShift
						{
							UserID = c.ID,
							ShiftID = shiftToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						UserShift? enrollmentToRemove = shiftToUpdate.UserShifts
							.FirstOrDefault(d => d.UserID == c.ID);
						if (enrollmentToRemove != null)
						{
							_context.Remove(enrollmentToRemove);
						}
					}
				}
			}
		}

		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> TrackPerformance(int id)
		{
			var groupClass = await _context.Shifts
				.Include(g => g.UserShifts).ThenInclude(e => e.User)
				.FirstOrDefaultAsync(m => m.ID == id);

			if (groupClass == null)
			{
				return NotFound();
			}

			var enrollmentsVM = groupClass.UserShifts.Where(e => e.User != null).Select(e => new EnrollmentVM
			{
				UserID = e.UserID,
				Volunteer = e.User.NameFormatted,
				ShowOrNot = e.NoShow,
				StartAt = e.StartAt,
				EndAt = e.EndAt
			}).ToList();

			return PartialView("_TrackPerformance", enrollmentsVM);
		}

		[HttpPost]
        [Authorize(Roles = "Admin, Planner")]
        public async Task<IActionResult> UpdatePerformance([FromBody] List<EnrollmentVM> enrollments)
		{
			if (enrollments == null || enrollments.Count == 0)
			{
				return Json(new { success = false, message = "No data received." });
			}

			try
			{
				foreach (var enrollmentVM in enrollments)
				{
					var volunteer = await _context.Volunteers.FirstOrDefaultAsync(m => m.ID == enrollmentVM.UserID);

					var userShifts = await _context.UserShifts.FirstOrDefaultAsync(e => e.ShiftID == enrollmentVM.ShiftID);

					var enrollment = await _context.UserShifts.Include(g => g.User)
						.FirstOrDefaultAsync(e => e.UserID == enrollmentVM.UserID && e.ShiftID == enrollmentVM.ShiftID);

					if (enrollmentVM.ShowOrNot == true && enrollmentVM.StartAt - enrollmentVM.EndAt != TimeSpan.Zero)
					{
						throw new InvalidOperationException("Cannot have work hours when marked as a No Show.");
					}

					if (enrollmentVM.ShowOrNot == false && enrollmentVM.StartAt > enrollmentVM.EndAt)
					{
						throw new InvalidOperationException("Start time cannot be after end time when the volunteer shows up.");
					}

					if (enrollment != null)
					{
						enrollment.NoShow = enrollmentVM.ShowOrNot;
						enrollment.StartAt = enrollmentVM.StartAt;
						enrollment.EndAt = enrollmentVM.EndAt;

						if (volunteer != null)
						{
							if (enrollment.EndAt > enrollment.StartAt)
							{
								enrollment.WorkingHourRecorded = true;
							}
							if (enrollment.EndAt == enrollment.StartAt) { enrollment.WorkingHourRecorded = false; }
						}
					}
				}
				foreach (var enrollmentVM in enrollments)
				{
					var volunteer = await _context.Volunteers.FirstOrDefaultAsync(m => m.ID == enrollmentVM.UserID);

					var userShifts = await _context.UserShifts.FirstOrDefaultAsync(e => e.ShiftID == enrollmentVM.ShiftID);
					var shift = await _context.Shifts.FirstOrDefaultAsync(e => e.ID == enrollmentVM.ShiftID);

					var enrollment = await _context.UserShifts.Include(g => g.User)
						.FirstOrDefaultAsync(e => e.UserID == enrollmentVM.UserID && e.ShiftID == enrollmentVM.ShiftID);

					var shift2 = await _context.Shifts
						.Include(d => d.Event)
						.AsNoTracking()
						.FirstOrDefaultAsync(d => d.ID == shift.ID);
					string Subject = "Volunteer working hours";

					string emailContent = $"Your working hours in shift {shift.TimeSummary} of event {shift2.Event.Name} are from {enrollment.StartAt} to {enrollment.EndAt}. ";

					var volunteers = _context.Volunteers;

					int folksCount = 0;
					try
					{
						//Send a Notice.
						List<EmailAddress> folks = (from p in volunteers

													where p.ID == volunteer.ID
													select new EmailAddress
													{
														Name = p.NameFormatted,
														Address = p.Email
													}).ToList();
						folksCount = folks.Count;
						if (folksCount > 0)
						{
							var msg = new EmailMessage()
							{
								ToAddresses = folks,
								Subject = Subject,
								Content = "<p>" + emailContent
							};
							await _emailSender.SendToManyAsync(msg);
							ViewData["Message"] = "Message sent to " + folksCount + " Manager"
								+ ((folksCount == 1) ? "." : "s.");
						}
						else
						{
							ViewData["Message"] = "Message NOT sent!  No Manager.";
						}
					}
					catch (Exception ex)
					{
						string errMsg = ex.GetBaseException().Message;
						ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Client"
							+ ((folksCount == 1) ? "" : "s") + " in the list.";
					}











				}



				await _context.SaveChangesAsync();

				






				return Json(new { success = true, message = "Performance updated successfully." });









			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Error updating performance: " + ex.Message });
			}
		}

		private void PopulateDropDown(Shift? shift = null)
		{
			ViewData["EventID"] = EventSelectList(shift?.EventID, Status.Active);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Canceled)
						 .ToList();

			ViewBag.StatusList = new SelectList(statusList);
		}

		//private void PopulateDropDown2(Shift? shift = null)
		//{
		//         // ViewData["EventID"] = new SelectList(_context.Events.OrderBy(e => e.Name), "ID", "Name");
		//         ViewData["EventID"] = EventSelectList(shift?.EventID, Status.Active);

		//          var statusList = Enum.GetValues(typeof(Status))
		//				 .Cast<Status>()
		//				 .Where(s => s == Status.Active || s == Status.Canceled || s == Status.Archived)
		//				 .ToList();

		//	ViewBag.StatusList = new SelectList(statusList);
		//}

		// method to put list of active volunteers into viewdata
		private void PopulateVolunteers()
		{
			ViewData["VolunteerID"] = new SelectList(_context
				.Volunteers
				.Where(v => v.Status == Status.Active)
				.OrderBy(v => v.LastName), "ID", "NameFormatted");
		}

		// Sign a volunteer up for a shift
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> ShiftSignUp(int shiftID, int volID)
		{
			if (User.IsInRole("Volunteer"))
			{
				volID = _context.Volunteers.FirstOrDefault(v => v.Email == User.Identity.Name).ID;
			}
			bool userShiftExists = _context.UserShifts
		.Any(us => us.UserID == volID && us.ShiftID == shiftID);

			if (userShiftExists)
			{
				TempData["ErrorMessage"] = "This volunteer is already signed up for this shift.";

				return RedirectToAction("Details", "Shift", new { id = shiftID });
			}
			// create new volunteer signup
			var userShift = new UserShift
			{
				UserID = volID,
				ShiftID = shiftID
			};

			var shift = _context.Shifts.Where(s => s.ID == shiftID).FirstOrDefault();

			// get date for success toast
			string date = _context.Shifts.Where(s => s.ID == shiftID).Select(s => s.StartAt.ToLongDateString()).FirstOrDefault();
			// get name for success toast
			string name = _context.Volunteers.Where(v => v.ID == volID).Select(v => v.NameFormatted).FirstOrDefault();
			// get event for success toast
			int eventID = _context.Shifts.Where(s => s.ID == shiftID).Select(s => s.EventID).FirstOrDefault();
			string event_ = _context.Events.Where(e => e.ID == eventID).Select(e => e.Name).FirstOrDefault();

			try
			{
				if (shift.Status == Status.Canceled)
				{
					throw new Exception("Cannot register for a cancelled shift");
				}
				_context.Add(userShift);
				await _context.SaveChangesAsync();
				AddSignUpToast(date, name, event_);

				return RedirectToAction($"Details", "Volunteer", new { id = volID });
			}
			catch (Exception ex)
			{
				if (ex.GetBaseException().Message.Contains("cancelled"))
				{
					ModelState.AddModelError("", "Cannot register ");
				}
				else
				{
					ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
				}
				return RedirectToAction("Details", "Shift", new { id = shiftID });
			}
		}

		public JsonResult GetShiftData()
		{
			var date = new DateTime(2025, 11, 30);

			var data = new Shift
			{
				ShiftDate = date,
				StartAt = date.AddHours(10),
				EndAt = date.AddHours(14),
				EventID = 5
			};
			return Json(data);
		}

		[Authorize(Roles = "Admin, Planner")]
		public IActionResult CreateMany(int EventID)
		{
			Shift shift = new Shift();

			ViewData["EventID"] = EventID;
			var eventData = _context.Events
		.Where(e => e.ID == EventID)
		.FirstOrDefault();

			ViewData["Event"] = $"{eventData.Name} ({eventData.StartDate:MM-dd-yyyy} - {eventData.EndDate:MM-dd-yyyy})";

			return View();
		}

		// POST: Shift/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		// POST: Shift/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Planner")]
		public async Task<IActionResult> CreateMany(
													[Bind("ID,ShiftDate,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift,
													List<Shift> templateShifts,
													List<Shift> customShifts,
													List<int> customDays)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var eventRecord = _context.Events
						.FirstOrDefault(e => e.ID == shift.EventID);
					if (eventRecord == null)
					{
						ModelState.AddModelError("", "Event not found.");
						return View(shift);
					}

					var shiftsToCreate = new List<Shift>();

					for (var date = eventRecord.StartDate; date <= eventRecord.EndDate; date = date.AddDays(1))
					{
						var dayOfWeek = (int)date.DayOfWeek;

						if (customDays != null && customDays.Contains(dayOfWeek))
						{
							for (int i = 0; i < customDays.Count; i++)
							{
								if (customDays[i] == dayOfWeek)
								{
									shiftsToCreate.Add(new Shift
									{
										ShiftDate = date,
										StartAt = customShifts[i].StartAt,
										EndAt = customShifts[i].EndAt,
										EventID = shift.EventID,
										VolunteersNeeded = customShifts[i].VolunteersNeeded
									});
								}
							}
						}
						else
						{
							foreach (var templateShift in templateShifts)
							{
								shiftsToCreate.Add(new Shift
								{
									ShiftDate = date,
									StartAt = templateShift.StartAt,
									EndAt = templateShift.EndAt,
									EventID = shift.EventID,
									VolunteersNeeded = templateShift.VolunteersNeeded
								});
							}
						}
					}

					foreach (var newShift in shiftsToCreate)
					{
						var sameShifts = _context.Shifts
							.Where(r => r.ShiftDate == newShift.ShiftDate && r.EventID == newShift.EventID);

						foreach (var existingShift in sameShifts)
						{
							if ((newShift.StartAt.TimeOfDay < existingShift.EndAt.TimeOfDay && newShift.StartAt.TimeOfDay >= existingShift.StartAt.TimeOfDay) ||
								(newShift.EndAt.TimeOfDay < existingShift.EndAt.TimeOfDay && newShift.EndAt.TimeOfDay > existingShift.StartAt.TimeOfDay))
							{
								throw new DbUpdateException("Unable to save changes. Remember, you cannot have overlapping shifts.");
							}
						}
					}

					_context.AddRange(shiftsToCreate);
					await _context.SaveChangesAsync();
                   
                    _toastNotification.AddSuccessToastMessage($"Shifts were successfully added to {eventRecord.Name}.");
                    return RedirectToAction("Index", new { EventID = shift.EventID });
                }
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("overlapping shifts"))
				{
					ModelState.AddModelError("", "Unable to save changes. Shifts overlap.");
				}
				else if (message.Contains("Event range"))
				{
					ModelState.AddModelError("", "Unable to save changes. Your date is out of Event range.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			ViewData["EventID"] = shift.EventID;
			var eventData = _context.Events
				.Where(e => e.ID == shift.EventID)
				.FirstOrDefault();

			ViewData["Event"] = $"{eventData.Name} ({eventData.StartDate:MM-dd-yyyy} - {eventData.EndDate:MM-dd-yyyy})";

			return View();
		}

		private bool ShiftExists(int id)
		{
			return _context.Shifts.Any(e => e.ID == id);
		}
	}
}