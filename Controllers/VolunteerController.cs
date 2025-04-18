﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.ViewModels;
using TomorrowsVoice_Toplevel.Utilities;
using static TomorrowsVoice_Toplevel.Utilities.EmailService;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin, Planner, Volunteer")]
	public class VolunteerController : ElephantController
	{
		private readonly TVContext _context;
		private readonly ApplicationDbContext _identityContext;
		private readonly IMyEmailSender _emailSender;
		private readonly UserManager<IdentityUser> _userManager;

		public VolunteerController(TVContext context, ApplicationDbContext identityContext, IMyEmailSender emailSender, UserManager<IdentityUser> userManager, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
			_identityContext = identityContext;
			_emailSender = emailSender;
			_userManager = userManager;
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> Index(string? SearchString, int? page, int? pageSizeID, string? actionButton, string? StatusFilter, string sortField = "Volunteer", string sortDirection = "asc")
		{
			string[] sortOptions = new[] { "Volunteer", "Hours Volunteered", "Shifts Attended", "Shifts Missed" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;
			Enum.TryParse(StatusFilter, out Status selectedDOW);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Archived)
						 .ToList();

			ViewBag.StatusList = new SelectList(statusList);

			var volunteers = _context.Volunteers
				.Include(v => v.UserShifts)
					.ThenInclude(us => us.Shift)
				.AsNoTracking();

			if (!String.IsNullOrEmpty(StatusFilter))
			{
				volunteers = volunteers.Where(p => p.Status == selectedDOW);

				// filter out archived singers if the user does not specifically select "archived"
				if (selectedDOW != Status.Archived)
				{
					volunteers = volunteers.Where(s => s.Status != Status.Archived);
				}
				numberFilters++;
			}
			else
			{
				volunteers = volunteers.Where(s => s.Status != Status.Archived);
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				volunteers = volunteers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
									   || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
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

				if (sortOptions.Contains(actionButton))//Change of sort is requested
				{
					if (actionButton == sortField) //Reverse order on same field
					{
						sortDirection = sortDirection == "asc" ? "desc" : "asc";
					}
					sortField = actionButton;//Sort by the button clicked
				}
			}
			if (sortField == "Volunteer")
			{
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				ViewData["partiSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers
						.OrderBy(v => v.LastName)
						.ThenBy(v => v.FirstName);
					ViewData["volunteerSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers
						.OrderByDescending(v => v.LastName)
						.ThenBy(v => v.FirstName);
					ViewData["volunteerSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Hours Volunteered")
			{
				ViewData["volunteerSort"] = "fa fa-solid fa-sort";
				ViewData["partiSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.UserShifts.Sum(us => us.EndAt.Ticks - us.StartAt.Ticks));
					ViewData["hourVolSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.UserShifts.Sum(us => us.EndAt.Ticks - us.StartAt.Ticks));
					ViewData["hourVolSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Shifts Attended")
			{
				ViewData["volunteerSort"] = "fa fa-solid fa-sort";
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.UserShifts.Count(us => us.NoShow == false));
					ViewData["partiSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.UserShifts.Count(us => us.NoShow == false));
					ViewData["partiSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Shifts Missed")
			{
				ViewData["volunteersSort"] = "fa fa-solid fa-sort";
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.UserShifts.Count(us => us.NoShow == true));
					ViewData["absenceSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.UserShifts.Count(us => us.NoShow == true));
					ViewData["absenceSort"] = "fa fa-solid fa-sort-down";
				}
			}

			//Set sort for next time
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers.AsNoTracking(), page ?? 1, pageSize);

			//var pagedVolunteers = volList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

			return View(pagedData);
		}

		[Authorize(Roles = "Admin")]
		public IActionResult Create()
		{
			return View();
		}

		// POST: Volunteer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(volunteer);
					await _context.SaveChangesAsync();

					AddSuccessToast(volunteer.NameFormatted);
					//_toastNotification.AddSuccessToastMessage($"{vounteer.NameFormatted} was successfully created.");
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

			return View(volunteer);
		}

		// GET: Volunteer/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers.Include(g => g.UserShifts).ThenInclude(e => e.Shift)
			   .FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}
			PopulateAssignedEnrollmentData(volunteer);
			return View(volunteer);
		}

		// POST: Volunteer/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions)
		{
			var volunteerToUpdate = await _context.Volunteers.Include(g => g.UserShifts).ThenInclude(e => e.Shift)
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (volunteerToUpdate == null)
			{
				return NotFound();
			}
			UpdateEnrollments(selectedOptions, volunteerToUpdate);
			// Try updating with posted values
			if (await TryUpdateModelAsync<Volunteer>(volunteerToUpdate,
					"",
					r => r.FirstName,
					r => r.LastName,
				   r => r.MiddleName,
				   r => r.Email,
					r => r.Phone
			))
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

			PopulateAssignedEnrollmentData(volunteerToUpdate);
			return View(volunteerToUpdate);
		}

		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var volunteer = await _context.Volunteers

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (volunteer != null)
				{
					_context.Volunteers.Remove(volunteer);

					// Here we are archiving a vounteer instead of deleting them
					//volunteer.Status = Status.Archived;
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

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Recover(int? id)
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

		// POST: Volunteer/Recover/5
		[HttpPost, ActionName("Recover")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RecoverConfirmed(int id)
		{
			var volunteer = await _context.Volunteers

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (volunteer != null)
				{
					//_context.Volunteers.Remove(vounteer);

					// Here we are archiving a vounteer instead of deleting them
					volunteer.Status = Status.Active;
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

		private void PopulateAssignedEnrollmentData(Volunteer volunteer)
		{
			//For this to work, you must have Included the child collection in the parent object
			var allOptions = _context.Shifts;
			var currentOptionsHS = new HashSet<int>(volunteer.UserShifts.Select(b => b.ShiftID));
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
						DisplayText = c.EndAt.ToString()
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.EndAt.ToString()
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		private void UpdateEnrollments(string[] selectedOptions, Volunteer volunteerToUpdate)
		{
			if (selectedOptions == null)
			{
				volunteerToUpdate.UserShifts = new List<UserShift>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(volunteerToUpdate.UserShifts.Select(b => b.ShiftID));
			foreach (var c in _context.Shifts)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						volunteerToUpdate.UserShifts.Add(new UserShift
						{
							ShiftID = c.ID,
							UserID = volunteerToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						UserShift? enrollmentToRemove = volunteerToUpdate.UserShifts
							.FirstOrDefault(d => d.ShiftID == c.ID);
						if (enrollmentToRemove != null)
						{
							_context.Remove(enrollmentToRemove);
						}
					}
				}
			}
		}

		[Authorize(Roles = "Admin")]
		public IActionResult DownloadVolunteers()
		{
			//Get the appointments
			var appts = _context.Volunteers
	.Include(v => v.UserShifts)
	.AsEnumerable()
	.OrderByDescending(v => v.HoursVolunteered)
	.Select(a => new
	{
		Name = a.NameFormatted,
		Hours = a.HoursVolunteered,
		Participation = a.ParticipationCount,
		Absences = a.absences,
		Phone = a.PhoneFormatted,
		Email = a.Email,
	});

			int numRows = appts.Count();

			if (numRows > 0) //We have data
			{
				//Create a new spreadsheet from scratch.
				using (ExcelPackage excel = new ExcelPackage())
				{
					//Note: you can also pull a spreadsheet out of the database if you
					//have saved it in the normal way we do, as a Byte Array in a Model
					//such as the UploadedFile class.
					//
					// Suppose...
					//
					// var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
					//
					//    //Pass the Byte[] FileContent to a MemoryStream
					//
					// using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
					// {
					//     ExcelPackage package = new ExcelPackage(memStream);
					// }

					var workSheet = excel.Workbook.Worksheets.Add("Appointments");

					//Note: Cells[row, column]
					workSheet.Cells[3, 1].LoadFromCollection(appts, true);

					//Style first column for dates

					//Style fee column for currency

					//Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
					//Make Date and Patient Bold
					workSheet.Cells[4, 1, numRows + 3, 2].Style.Font.Bold = true;

					//Note: these are fine if you are only 'doing' one thing to the range of cells.
					//Otherwise you should USE a range object for efficiency

					//Set Style and backgound colour of headings
					using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
					{
						headings.Style.Font.Bold = true;
						var fill = headings.Style.Fill;
						fill.PatternType = ExcelFillStyle.Solid;
						fill.BackgroundColor.SetColor(Color.LightBlue);
					}

					////Boy those notes are BIG!
					////Lets put them in comments instead.

					//Autofit columns
					workSheet.Cells.AutoFitColumns();
					//Note: You can manually set width of columns as well
					//workSheet.Column(7).Width = 10;

					//Add a title and timestamp at the top of the report
					workSheet.Cells[1, 1].Value = "Volunteers Report";
					using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
					{
						Rng.Merge = true; //Merge columns start and end range
						Rng.Style.Font.Bold = true; //Font should be bold
						Rng.Style.Font.Size = 18;
						Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}
					//Since the time zone where the server is running can be different, adjust to
					//Local for us.
					DateTime utcDate = DateTime.UtcNow;
					TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
					DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
					using (ExcelRange Rng = workSheet.Cells[2, 7])
					{
						Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
							localDate.ToShortDateString();
						Rng.Style.Font.Bold = true; //Font should be bold
						Rng.Style.Font.Size = 12;
						Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
					}

					//Ok, time to download the Excel

					try
					{
						Byte[] theData = excel.GetAsByteArray();
						string filename = "Volunteers Report.xlsx";
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

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Email(string[] selectedOptions, string Subject, string emailContent)
		{
			var allOptions = _context.Volunteers;
			var currentOptionsHS = new HashSet<int>();
			//Instead of one list with a boolean, we will make two lists
			var selected = new List<ListOptionVM>();
			var available = new List<ListOptionVM>();

			var select1 = new List<Volunteer>();
			foreach (var c in allOptions)
			{
				if (currentOptionsHS.Contains(c.ID))
				{
					selected.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = c.NameFormatted,
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

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptions = new HashSet<int>(select1.Select(b => b.ID));
			foreach (var c in _context.Volunteers)
			{
				if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
				{
					if (!currentOptions.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
					{
						select1.Add(new Volunteer
						{
							ID = c.ID,
							Email = c.Email,
						});
					}
				}
				else //not selected
				{
					if (currentOptions.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
					{
						Volunteer? enrollmentToRemove = select1
							.FirstOrDefault(d => d.ID == c.ID);
						if (enrollmentToRemove != null)
						{
							select1.Remove(new Volunteer
							{
								ID = c.ID,
								Email = c.Email,
							});
						}
					}
				}
			}

			if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
			{
				ViewData["Message"] = "You must enter both a Subject and some message Content before" +
					" sending the message.";
			}
			else
			{
				int folksCount = 0;
				try
				{
					//Send a Notice.
					List<EmailAddress> folks = (from p in select1

												where p.Email != null
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
							Content = "<p>" + emailContent + "</p><p>Please access the <strong>Tomorrows Voice</strong> web site to review.</p>"
						};
						await _emailSender.SendToManyAsync(msg);
						ViewData["Message"] = "Message sent to " + folksCount + " Client"
							+ ((folksCount == 1) ? "." : "s.");
					}
					else
					{
						ViewData["Message"] = "Message NOT sent!  No Client.";
					}
				}
				catch (Exception ex)
				{
					string errMsg = ex.GetBaseException().Message;
					ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Client"
						+ ((folksCount == 1) ? "" : "s") + " in the list.";
				}
			}
			return View();
		}

		public async Task<IActionResult> EmailNotice(int shiftID, int volunteerID)

		{
			var volunteer = await _context.Volunteers.FirstOrDefaultAsync(m => m.ID == volunteerID);

			var shift = await _context.Shifts.Include(a => a.Event).FirstOrDefaultAsync(m => m.ID == shiftID);

			string Subject = "Sign off shift ";

			string emailContent = $"Volunteer: {volunteer.NameFormatted}  sign off event :{shift.Event.Name} shift: {shift.TimeSummary}  ";

			var volunteers = _context.Volunteers;

			int folksCount = 0;
			try
			{
				//Send a Notice.
				List<EmailAddress> folks = (from p in volunteers

											where p.ID == 1000
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

			//return View();
			return RedirectToAction("SignOffShift", new { volunteerID = volunteerID, shiftID = shiftID });
		}

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers
				.Include(v => v.UserShifts)
				.ThenInclude(v => v.Shift)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}
			var _user = await _userManager.FindByEmailAsync(volunteer.Email); // IdentityUser
			if (_user != null)
			{
				var UserRoles = (List<string>)await _userManager.GetRolesAsync(_user); // Current roles user is in
				ViewBag.UserRoles = UserRoles;
			}
			return View(volunteer);
		}

		public PartialViewResult ListOfShiftDetails(int id)

		{
			var volunteer = _context.Volunteers
							.FirstOrDefault(v => v.ID == id);
			ViewData["Volunteer"] = volunteer;
			var shifts = _context.Shifts
				  .Include(a => a.UserShifts).Include(c => c.Event)
				  //.Where(vs => vs.Status != Status.Archived)
				  .Where(r => r.UserShifts.Any(ra => ra.UserID == id))
				  .OrderBy(r => r.ShiftDate)
				  .ToList();

			return PartialView("_ListOfShiftDetails", shifts);
		}

		public async Task<IActionResult> ShiftIndex(int id)
		{
			var volunteer = _context.Volunteers
							.FirstOrDefault(v => v.ID == id);

			if (volunteer == null)
			{
				return NotFound();
			}
			ViewData["Volunteer"] = volunteer;

			var volunteerShifts = _context.UserShifts.Include(a => a.Shift)
								   .Where(vs => vs.UserID == volunteer.ID).Where(vs => vs.Shift.Status != Status.Archived)
								   .Select(vs => vs.ShiftID)
								   .ToList();

			ViewData["VolunteerShifts"] = volunteerShifts;

			var tVContext = _context.Shifts.Include(s => s.Event);
			return View(await tVContext.ToListAsync());
		}

		// GET: Shift/AddShift/5
		public async Task<IActionResult> SignOffShift(int volunteerId, int shiftId)
		{
			if (shiftId == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
				.Include(s => s.Event)
				.FirstOrDefaultAsync(m => m.ID == shiftId);
			if (shift == null)
			{
				return NotFound();
			}
			ViewData["volunteerId"] = volunteerId;
			return View(shift);
		}

		[HttpPost, ActionName("SignOffShift")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SignOffShiftConfirmed(int volunteerId, int shiftId)
		{
			var userShift = await _context.UserShifts
								   .FirstOrDefaultAsync(us => us.UserID == volunteerId && us.ShiftID == shiftId);

			var volunteer = _context.Volunteers
							.FirstOrDefault(v => v.ID == volunteerId);

			var Shift = _context.Shifts
								  .FirstOrDefault(v => v.ID == shiftId);

			// get info for toast notification
			string shiftDate = Shift.ShiftDate.ToLongDateString();
			string volunteerName = volunteer.NameFormatted;
			int eventID = _context.Shifts.Where(s => s.ID == shiftId).Select(s => s.EventID).FirstOrDefault();
			string eventName = _context.Events.Where(e => e.ID == eventID).Select(e => e.Name).FirstOrDefault();

			try
			{
				if (userShift != null)
				{
					_context.UserShifts.Remove(userShift);
					await _context.SaveChangesAsync();
					AddCancelledToast(shiftDate, volunteerName, eventName);
					//AddSuccessToast($"Shift {Shift.ShiftDate} successfully removed for volunteer {volunteer.NameFormatted}.");
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return RedirectToAction("VolunteerforUser", "Volunteer");
		}

		public JsonResult GetVolunteerData()
		{
			var data = new Volunteer
			{
				FirstName = "Grace",
				LastName = "Hill",
				Email = "Grace_Hill@gmail.com",
				Phone = "9051231231",
			};
			return Json(data);
		}

		public async Task<IActionResult> IndexVolunteer(string? SearchString, int? page, int? pageSizeID, string? actionButton, string? StatusFilter, string sortField = "Volunteer", string sortDirection = "asc")
		{
			string[] sortOptions = new[] { "Volunteer", "Hours Volunteered", "Participation", "Absences" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;
			Enum.TryParse(StatusFilter, out Status selectedDOW);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Archived)
						 .ToList();

			ViewBag.StatusList = new SelectList(statusList);
			var volunteers = _context.Volunteers.AsNoTracking();
			if (!String.IsNullOrEmpty(StatusFilter))
			{
				volunteers = volunteers.Where(p => p.Status == selectedDOW);

				// filter out archived singers if the user does not specifically select "archived"
				if (selectedDOW != Status.Archived)
				{
					volunteers = volunteers.Where(s => s.Status != Status.Archived);
				}
				numberFilters++;
			}
			else
			{
				volunteers = volunteers.Where(s => s.Status != Status.Archived);
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				volunteers = volunteers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
									   || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
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

				if (sortOptions.Contains(actionButton))//Change of sort is requested
				{
					if (actionButton == sortField) //Reverse order on same field
					{
						sortDirection = sortDirection == "asc" ? "desc" : "asc";
					}
					sortField = actionButton;//Sort by the button clicked
				}
			}
			if (sortField == "Volunteer")
			{
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				ViewData["partiSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers
						.OrderBy(v => v.LastName)
						.ThenBy(v => v.FirstName);
					ViewData["volunteerSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers
						.OrderByDescending(v => v.LastName)
						.ThenBy(v => v.FirstName);
					ViewData["volunteerSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Hours Volunteered")
			{
				ViewData["volunteerSort"] = "fa fa-solid fa-sort";
				ViewData["partiSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.HoursVolunteered);
					ViewData["hourVolSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.HoursVolunteered);
					ViewData["hourVolSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Participation")
			{
				ViewData["volunteerSort"] = "fa fa-solid fa-sort";
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				ViewData["absenceSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.ParticipationCount);
					ViewData["partiSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.ParticipationCount);
					ViewData["partiSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Absences")
			{
				ViewData["volunteersSort"] = "fa fa-solid fa-sort";
				ViewData["hourVolSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					volunteers = volunteers.OrderBy(v => v.absences);
					ViewData["absenceSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					volunteers = volunteers.OrderByDescending(v => v.absences);
					ViewData["absenceSort"] = "fa fa-solid fa-sort-down";
				}
			}

			//Set sort for next time
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// Page for Selecting a volunteer
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> VolunteerSelect()
		{
			ViewData["VolunteerID"] = new SelectList(_context
				.Volunteers
				.Where(v => v.Status == Status.Active)
				.OrderBy(v => v.LastName), "ID", "NameFormatted");
			return View();
		}

		public async Task<IActionResult> TrackPerformance(int id, int volunteerId)
		{
			var groupClass = await _context.Shifts
				.Include(g => g.UserShifts).ThenInclude(e => e.User)
				.FirstOrDefaultAsync(m => m.ID == id);

			if (groupClass == null)
			{
				return NotFound();
			}

			var enrollmentsVM = groupClass.UserShifts.Where(e => e.User.ID == volunteerId).Select(e => new EnrollmentVM
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

					if (enrollmentVM.ShowOrNot == false && enrollmentVM.StartAt >= enrollmentVM.EndAt)
					{
						throw new InvalidOperationException("Start time cannot be after end time when the volunteer shows up.");
					}

					if (enrollment != null)
					{
						enrollment.NoShow = enrollmentVM.ShowOrNot;
						enrollment.StartAt = enrollmentVM.StartAt;
						enrollment.EndAt = enrollmentVM.EndAt;
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

		#region VolunteerRoles

		[ActionName("PromoteDemote")]
		public async Task<IActionResult> PromoteDemoteVolunteer(int id)
		{
			var volunteer = _context.Volunteers.FirstOrDefault(v => v.ID == id);
			var _user = await _userManager.FindByEmailAsync(volunteer.Email); // IdentityUser
			if (_user != null)
			{
				var UserRoles = (List<string>)await _userManager.GetRolesAsync(_user); // Current roles user is in
				IList<IdentityRole> allRoles = _identityContext.Roles.ToList<IdentityRole>();
				if (UserRoles.Contains(allRoles.FirstOrDefault(r => r.Name == "Planner").Name)) // If User is already a planner
				{
					await _userManager.RemoveFromRoleAsync(_user, "Planner");
					_toastNotification.AddSuccessToastMessage($"Successfuly revoked event planner from {volunteer.NameFormatted}.");
				}
				else
				{
					await _userManager.AddToRoleAsync(_user, "Planner");
					_toastNotification.AddSuccessToastMessage($"Successfuly assigned {volunteer.NameFormatted} to event planner.");
				}
			}
			return RedirectToAction("Details", new { volunteer.ID });
		}

		#endregion VolunteerRoles

		private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}