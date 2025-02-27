using System;
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

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class VolunteerController : ElephantController
	{
		private readonly TVContext _context;

		public VolunteerController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: Volunteer
		public async Task<IActionResult> Index(string? SearchString, int? page, int? pageSizeID, string? actionButton, string sortField = "Volunteer", string sortDirection = "asc")
		{
            string[] sortOptions = new[] { "Volunteer","Hours Volunteered","Participation","Absences" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			var volunteers = _context.Volunteers.AsNoTracking();

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
	

		// GET: Volunteer/Details/5
		public async Task<IActionResult> Details(int? id)
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

		// GET: Volunteer/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Volunteer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					volunteer.ID = _context.GetNextID();
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
				if (message.Contains("UNIQUE") && message.Contains("volunteer.Email"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, " +
						"you cannot have duplicate Name and Email.");
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
					r => r.Phone,
					r => r.Status))
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

		// GET: Volunteer/Delete/5
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
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var volunteer = await _context.Volunteers

			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (volunteer != null)
				{
					//_context.Volunteers.Remove(vounteer);

					// Here we are archiving a vounteer instead of deleting them
					volunteer.Status = Status.Archived;
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
        public IActionResult DownloadVolunteers()
        {
            //Get the appointments
            var appts = from a in _context.Volunteers
                        
                        orderby a.HoursVolunteered descending
                        select new
                        {
                            
                            Name = a.NameFormatted,
                            Hours = a.HoursVolunteered,
                            Participation = a.ParticipationCount,
                            Absences = a.absences,
                            Phone = a.PhoneFormatted,
                            Email = a.Email,
                           
                        };
            //How many rows?
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

	private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}