using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class SingerController : ElephantController
	{
		private readonly TVContext _context;

		public SingerController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}
        private Director? GetDirectorFromUser()
        {
            if (!User.IsInRole("Director"))
                return null;

            return _context.Directors
                    .Where(d => d.Email == User.Identity.Name.ToString())
                    .FirstOrDefault();
        }
        // GET: Singer
        public async Task<IActionResult> Index(string? SearchString, int? ChapterID, int? page, int? pageSizeID, string? StatusFilter,
			string? actionButton, string sortDirection = "asc", string sortField = "Singer")
		{

            Director? dUser = GetDirectorFromUser();
            // Sort Options
            string[] sortOptions = new[] { "Singer", "Chapter" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;
			Enum.TryParse(StatusFilter, out Status selectedDOW);
			
			PopulateDropDownLists();
			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Inactive || s == Status.Archived)
						 .ToList();


			ViewBag.StatusList = new SelectList(statusList);

			var singers = _context.Singers
				.Include(s => s.Chapter).ThenInclude(c => c.City)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking();

			if (!String.IsNullOrEmpty(StatusFilter))
			{
				singers = singers.Where(p => p.Status == selectedDOW);

				// filter out archived singers if the user does not specifically select "archived"
				if (selectedDOW != Status.Archived)
				{
					singers = singers.Where(s => s.Status != Status.Archived);
				}
				numberFilters++;
			}
			// filter out singers even if status filter has not been set
			else
			{
				singers = singers.Where(s => s.Status != Status.Archived);
			}
            if (dUser != null)
            {
                singers = singers.Where(r => r.ChapterID == dUser.ChapterID);
            }
            if (ChapterID.HasValue)
			{
				singers = singers.Where(s => s.ChapterID == ChapterID);
				numberFilters++;
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				singers = singers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
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
			//Now we know which field and direction to sort by
			if (sortField == "Singer")
			{
				ViewData["chapterSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					singers = singers
						.OrderBy(s => s.LastName)
						.ThenBy(s => s.FirstName);
					ViewData["singerSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					singers = singers
						.OrderByDescending(s => s.LastName)
						.ThenBy(s => s.FirstName);
					ViewData["singerSort"] = "fa fa-solid fa-sort-down";
				}
			}
			else if (sortField == "Chapter")
			{
				ViewData["singerSort"] = "fa fa-solid fa-sort";
				if (sortDirection == "asc")
				{
					singers = singers
						.OrderBy(s => s.Chapter.City.Name);
					ViewData["chapterSort"] = "fa fa-solid fa-sort-up";
				}
				else
				{
					singers = singers
						.OrderByDescending(s => s.Chapter.City.Name);
					ViewData["chapterSort"] = "fa fa-solid fa-sort-down";
				}
			}

			//Set sort for next time
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;

			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Singer>.CreateAsync(singers.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// GET: Singer/Details/5
		public async Task<IActionResult> Details(int? id, int? chapterID)
		{
			if (id == null)
			{
				return NotFound();
			}

			var singer = await _context.Singers
			   .Include(s => s.Chapter).ThenInclude(c => c.City)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ID == id);
			if (singer == null)
			{
				return NotFound();
			}
			ViewBag.ChapterID = chapterID;
			return View(singer);
		}

		// GET: Singer/Create
		public IActionResult Create()
		{
			Singer singer = new Singer();
			Director? dUser = GetDirectorFromUser();
			if (dUser != null)
			{
				
				singer.ChapterID = dUser.ChapterID;
				
				
			}
			PopulateDropDownLists();
			return View(singer);
		}

		// POST: Singer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,ContactName,Phone,Email,Note,ChapterID,Status")] Singer singer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(singer);
					await _context.SaveChangesAsync();
					AddSuccessToast(singer.NameFormatted);
					//_toastNotification.AddSuccessToastMessage($"{singer.NameFormatted} was successfully created.");
					return RedirectToAction("Details", new { singer.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Singers.Email"))
				{
					ModelState.AddModelError("", "Unable to save changes. Remember, " +
						"you cannot have duplicate Name and Email.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			PopulateDropDownLists(singer);
			return View(singer);
		}

		// GET: Singer/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var singer = await _context.Singers
				.Include(r => r.Chapter).ThenInclude(c => c.City)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
				.FirstOrDefaultAsync(m => m.ID == id);

		
			if (singer == null)
			{
				return NotFound();
			}
			PopulateDropDownLists(singer);
			return View(singer);
		}

		// POST: Singer/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var singerToUpdate = await _context.Singers
			   .Include(r => r.Chapter).ThenInclude(c => c.City)
			   .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (singerToUpdate == null)
			{
				return NotFound();
			}

			//Put the original RowVersion value in the OriginalValues collection for the entity
			//_context.Entry(singerToUpdate).Property("RowVersion").OriginalValue = RowVersion;

			// Try updating with posted values
			if (await TryUpdateModelAsync<Singer>(singerToUpdate, "",
				r => r.ContactName, r => r.Note, r => r.Email, r => r.Phone, r => r.FirstName, r => r.MiddleName, r => r.LastName,
				r => r.Status, r => r.ChapterID))
			{
				try
				{
					await _context.SaveChangesAsync();
					AddSuccessToast(singerToUpdate.NameFormatted);
					return RedirectToAction("Details", new { singerToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var exceptionEntry = ex.Entries.Single();
					var clientValues = (Singer)exceptionEntry.Entity;
					var databaseEntry = exceptionEntry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError("",
							"Unable to save changes. The Singer was deleted by another user.");
					}
					else
					{
						var databaseValues = (Singer)databaseEntry.ToObject();
						if (databaseValues.FirstName != clientValues.FirstName)
							ModelState.AddModelError("FirstName", "Current value: "
								+ databaseValues.FirstName);
						if (databaseValues.MiddleName != clientValues.MiddleName)
							ModelState.AddModelError("MiddleName", "Current value: "
								+ databaseValues.MiddleName);
						if (databaseValues.LastName != clientValues.LastName)
							ModelState.AddModelError("LastName", "Current value: "
								+ databaseValues.LastName);
						if (databaseValues.Email != clientValues.Email)
							ModelState.AddModelError("Email", "Current value: "
								+ databaseValues.Email);
						if (databaseValues.Phone != clientValues.Phone)
							ModelState.AddModelError("Phone", "Current value: "
								+ databaseValues.PhoneFormatted);
						if (databaseValues.ContactName != clientValues.ContactName)
							ModelState.AddModelError("ContactName", "Current value: "
								+ databaseValues.ContactName);
						if (databaseValues.Note != clientValues.Note)
							ModelState.AddModelError("Note", "Current value: "
								+ databaseValues.Note);
						if (databaseValues.ChapterID != clientValues.ChapterID)
						{
							Chapter? chapter = await _context.Chapters.FirstOrDefaultAsync(c => c.ID == clientValues.ChapterID);
							string cName = chapter?.City.Name ?? "Unknown";
							ModelState.AddModelError("ChapterID", "Current value: "
								+ cName);
						}

						if (databaseValues.Status != clientValues.Status)
							ModelState.AddModelError("Status", "Current value: "
								+ databaseValues.Status.ToString());

						ModelState.AddModelError(string.Empty, "The record you attempted to edit "
								+ "was modified by another user after you received your values. The "
								+ "edit operation was canceled and the current values in the database "
								+ "have been displayed. If you still want to save your version of this record, click "
								+ "the Save button again. Otherwise click the 'Back to Singer List' hyperlink.");

						// Update RowVersion from the Database and remove the RowVersion error.
						singerToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
						ModelState.Remove("RowVersion");
					}
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE") && message.Contains("Singers.Email"))
					{
						ModelState.AddModelError("", "Unable to save changes. Remember, " +
							"you cannot have duplicate Name and Email.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.GetBaseException().Message.ToString());
				}
			}

			PopulateDropDownLists(singerToUpdate);
			return View(singerToUpdate);
		}

		// GET: Singer/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var singer = await _context.Singers
				.Include(s => s.Chapter).ThenInclude(c => c.City)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ID == id);
			if (singer == null)
			{
				return NotFound();
			}

			return View(singer);
		}

		// POST: Singer/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var singer = await _context.Singers
			   .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (singer != null)
				{
					//_context.Singers.Remove(singer);

					// Here we are archiving a singer instead of deleting them
					singer.Status = Status.Archived;
					await _context.SaveChangesAsync();
					AddSuccessToast(singer.NameFormatted);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(singer);
		}




		public async Task<IActionResult> Recover(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var singer = await _context.Singers
				.Include(s => s.Chapter).ThenInclude(c => c.City)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ID == id);
			if (singer == null)
			{
				return NotFound();
			}

			return View(singer);
		}

		// POST: Rehearsal/Recover/5
		[HttpPost, ActionName("Recover")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RecoverConfirmed(int id)
		{
			var singer = await _context.Singers
			   .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
			   .FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (singer != null)
				{
					//_context.Singers.Remove(singer);

					// Here we are archiving a singer instead of deleting them
					singer.Status = Status.Active;
					await _context.SaveChangesAsync();
					AddSuccessToast(singer.NameFormatted);
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(singer);
		}

		// GET: Singer/Upload/
		public async Task<IActionResult> Upload()
		{
			return View();
		}

		public PartialViewResult ListOfRehearsalDetails(int id)
		{
			var rehearsals = _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter).ThenInclude(c=>c.City)
				.Where(r => r.RehearsalAttendances.Any(ra => ra.SingerID == id))
				.OrderBy(r => r.RehearsalDate)
				.ToList();
			ViewBag.SingerID = id;
			return PartialView("_ListOfRehearsals", rehearsals);
		}

		private SelectList ChapterSelectList(int? selectedId)
		{
			return new SelectList(_context
				.Cities
				.OrderBy(c => c.Name), "ID", "Name", selectedId);
			//return new SelectList(_context.Chapters
			//	.OrderBy(c => c.City.Name), "ID", "Name", selectedId);
			/*if (ViewData["ActionName"].ToString() == "Index")
			{
			}
			else
			{
				var items = _context.Chapters
				.OrderBy(c => c.Name)
				.Select(c => new SelectListItem
				{
					Value = c.ID.ToString(),
					Text = c.Name
				}).ToList();
				items.Insert(0, new SelectListItem(){ Text = "Select Chapter"});
				return new SelectList(items, "Value", "Text", selectedId);
			}*/
		}

		public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
		{
			string feedBack = string.Empty;
			if (theExcel != null)
			{
				string mimeType = theExcel.ContentType;
				long fileLength = theExcel.Length;
				if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
				{
					if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
					{
						ExcelPackage excel;
						using (var memoryStream = new MemoryStream())
						{
							await theExcel.CopyToAsync(memoryStream);
							excel = new ExcelPackage(memoryStream);
						}
						var workSheet = excel.Workbook.Worksheets[0];
						var start = workSheet.Dimension.Start;
						var end = workSheet.Dimension.End;
						int successCount = 0;
						int errorCount = 0;
						if (workSheet.Cells[1, 1].Text == "FirstName" && workSheet.Cells[1, 2].Text == "MiddleName" && workSheet.Cells[1, 3].Text == "LastName" &&
							workSheet.Cells[1, 4].Text == "Email" && workSheet.Cells[1, 5].Text == "ContactName" && workSheet.Cells[1, 6].Text == "Phone" &&
							workSheet.Cells[1, 7].Text == "Chapter" && workSheet.Cells[1, 8].Text == "Note")
						{
							for (int row = start.Row + 1; row <= end.Row; row++)
							{
								Singer singer = new Singer();
								try
								{
									if (string.IsNullOrEmpty(workSheet.Cells[row, 1].Text) || // FirstName
										string.IsNullOrEmpty(workSheet.Cells[row, 3].Text) || // LastName
										string.IsNullOrEmpty(workSheet.Cells[row, 4].Text) || // Email
										string.IsNullOrEmpty(workSheet.Cells[row, 5].Text) || // ContactName
										string.IsNullOrEmpty(workSheet.Cells[row, 6].Text))   // Phone
									{
										feedBack += $"Error: Missing required information in row {row}. Please ensure FirstName, LastName, Email, ContactName, and Phone are provided.<br />";
										errorCount++;
										continue; // Skip this row if any required field is missing
									}
									singer.FirstName = workSheet.Cells[row, 1].Text;
									singer.MiddleName = workSheet.Cells[row, 2].Text;
									singer.LastName = workSheet.Cells[row, 3].Text;
									singer.Email = workSheet.Cells[row, 4].Text;
									singer.ContactName = workSheet.Cells[row, 5].Text;
									singer.Phone = workSheet.Cells[row, 6].Text;
									singer.Note = workSheet.Cells[row, 8].Text;
									string chapterName = workSheet.Cells[row, 7].Text;
									singer.Status = Status.Active;

									var chapter = await _context.Chapters
										.FirstOrDefaultAsync(c => c.City.Name == chapterName);

									if (chapter != null)
									{
										singer.ChapterID = chapter.ID;
									}
									else
									{
										feedBack += "Error: Chapter '" + chapterName + "' not found for singer " + singer.NameFormatted + ".<br />";
										errorCount++;
										continue;
									}

									_context.Singers.Add(singer);
									_context.SaveChanges();
									successCount++;
								}
								catch (DbUpdateException dex)
								{
									errorCount++;
									if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
									{
										feedBack += "Error: Record " + singer.NameFormatted +
											" was rejected as a duplicate." + "<br />";
									}
									else
									{
										feedBack += "Error: Record " + singer.NameFormatted +
											" caused a database error." + "<br />";
									}

									_context.Remove(singer);
								}
								catch (Exception ex)
								{
									errorCount++;
									if (ex.GetBaseException().Message.Contains("correct format"))
									{
										feedBack += "Error: Record " + singer.NameFormatted
											+ " was rejected becuase it was not in the correct format." + "<br />";
									}
									else
									{
										feedBack += "Error: Record " + singer.NameFormatted
											+ " caused and error." + "<br />";
									}
								}
							}
							feedBack += "Finished Importing " + (successCount + errorCount).ToString() +
								" Records with " + successCount.ToString() + " inserted and " +
								errorCount.ToString() + " rejected";
						}
						else
						{
							feedBack = "Error: You may have selected the wrong file to upload.<br /> " +
								"Remember, you must have the order and content of the first row  " +
								"must be exactly the same as instructions.";
						}
					}
					else
					{
						feedBack = "Error: That file is not an Excel spreadsheet.";
					}
				}
				else
				{
					feedBack = "Error:  file appears to be empty";
				}
			}
			else
			{
				feedBack = "Error: No file uploaded";
			}

			TempData["Feedback"] = feedBack + "<br /><br />";

			//Note that we are assuming that you are using the Preferred Approach to Lookup Values
			//And the custom LookupsController
			return RedirectToAction(nameof(Upload));
		}

		private void PopulateDropDownLists(Singer? singer = null)
		{
			ViewData["ChapterID"] = CitySelectList(singer?.ChapterID, Status.Active);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Inactive )
						 .ToList();


			ViewBag.StatusList = new SelectList(statusList);
		}

		private bool SingerExists(int id)
		{
			return _context.Singers.Any(e => e.ID == id);
		}
	}
}