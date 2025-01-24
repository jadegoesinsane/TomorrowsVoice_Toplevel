using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.ViewModels;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.CustomControllers;

using System.Drawing;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class RehearsalController : ElephantController
	{
		private readonly TVContext _context;

		public RehearsalController(TVContext context)
		{
			_context = context;
		}

		// GET: Rehearsal
		public async Task<IActionResult> Index(
			string? SearchString,
			int? DirectorID,
			int? ChapterID,
			string? actionButton,
			int? page,
			int? pageSizeID,
			string sortDirection = "asc",
			string sortField = "Date")
		{
			// Sort Options
			string[] sortOpts = new[] { "Date" };

			// Counts number of filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numFilters = 0;

			PopulateDropDown();

			var rehearsals = _context.Rehearsals
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.AsNoTracking();

			// Filters
			if (DirectorID.HasValue)
			{
				rehearsals = rehearsals.Where(r => r.DirectorID == DirectorID);
				numFilters++;
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				rehearsals = rehearsals.Where(r => r.Note.ToUpper().Contains(SearchString.ToUpper()));
				numFilters++;
			}
			if (ChapterID.HasValue)
			{
				rehearsals = rehearsals.Where(r => r.Director.ChapterID == ChapterID);
				numFilters++;
			}

			// Show how many filters are applied
			if (numFilters != 0)
			{
				ViewData["Filtering"] = " btn-danger";
				ViewData["numberFilters"] = "(" + numFilters.ToString()
					+ " Filter" + (numFilters > 1 ? "s" : "") + " Applied)";
			}

			// Check for sorting change
			if (!String.IsNullOrEmpty(actionButton))
			{
				page = 1;
				if (sortOpts.Contains(actionButton))
				{
					sortDirection = sortDirection == "asc" ? "desc" : "asc";
				}
				sortField = actionButton;
			}

			// Sorting
			if (sortField == "Date")
			{
				if (sortDirection == "asc")
				{
					rehearsals = rehearsals
						.OrderByDescending(r => r.RehearsalDate)
						.ThenByDescending(r => r.RehearsalDate);
				}
				else
				{
					rehearsals = rehearsals
						.OrderBy(r => r.RehearsalDate)
						.ThenBy(r => r.RehearsalDate);
				}
			}

			// View data for next sort
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;

			// Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Rehearsal>.CreateAsync(rehearsals.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// GET: Rehearsal/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var rehearsal = await _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d=>d.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (rehearsal == null)
			{
				return NotFound();
			}

			return View(rehearsal);
		}

		// GET: Rehearsal/Create
		public IActionResult Create(int? chapterSelect)
		{
			Rehearsal rehearsal = new Rehearsal();

			ViewBag.Chapters = new SelectList(_context.Chapters, "ID", "Name", chapterSelect);
			PopulateDropDown(rehearsal);

			// Get all clients and filter by membership if a filter is applied
			PopulateAttendance(chapterSelect, rehearsal);

			return View();
		}

		// POST: Rehearsal/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("RehearsalDate,StartTime,EndTime,Note,DirectorID")] string[] selectedOptions, Rehearsal rehearsal, int? chapterSelect)
		{
			try
			{
				UpdateAttendance(selectedOptions, rehearsal);
				if (ModelState.IsValid)
				{
					_context.Add(rehearsal);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (RetryLimitExceededException /* dex */)//This is a Transaction in the Database!
			{
				ModelState.AddModelError("", "Unable to save changes after multiple attempts. " +
					"Try again, and if the problem persists, see your system administrator.");
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Rehearsals.RehearsalDate"))
				{
					ModelState.AddModelError("RehearsalDate", "Unable to save changes. Remember, " +
						"directors can only have one rehearsal a day.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
				}
			}

			ViewBag.Chapters = new SelectList(_context.Chapters, "ID", "Name", chapterSelect);

			// Get all clients and filter by membership if a filter is applied
			PopulateAttendance(chapterSelect, rehearsal);

			PopulateDropDown(rehearsal);
			return View(rehearsal);
		}

		// GET: Rehearsal/Edit/5
		public async Task<IActionResult> Edit(int? id, int? chapterSelect)
		{
			if (id == null)
			{
				return NotFound();
			}

			var rehearsal = await _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (rehearsal == null)
			{
				return NotFound();
			}
			PopulateDropDown(rehearsal);
			ViewBag.Chapters = new SelectList(_context.Chapters, "ID", "Name", chapterSelect);
			PopulateAttendance(chapterSelect, rehearsal);
			return View(rehearsal);
		}

		// POST: Rehearsal/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions, int? chapterSelect) //"ID,RehearsalDate,StartTime,EndTime,Note,ChapterID"
		{
			var rehearsalToUpdate = await _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.FirstOrDefaultAsync(m => m.ID == id);

			if (rehearsalToUpdate == null)
			{
				return NotFound();
			}
			try
			{
				UpdateAttendance(selectedOptions, rehearsalToUpdate);

				if (await TryUpdateModelAsync(rehearsalToUpdate,
					"",
					r => r.RehearsalDate,
					r => r.StartTime,
					r => r.EndTime,
					r => r.Note,
					r => r.DirectorID))
				{
					try
					{
						await _context.SaveChangesAsync();
						return RedirectToAction("Details", new { rehearsalToUpdate.ID });
					}
					catch (RetryLimitExceededException)
					{
						ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
					}
					catch (DbUpdateConcurrencyException)
					{
						if (!RehearsalExists(rehearsalToUpdate.ID))
						{
							return NotFound();
						}
						else
						{
							ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetBaseException().ToString());
			}

			PopulateDropDown(rehearsalToUpdate);
			ViewBag.Chapters = new SelectList(_context.Chapters, "ID", "Name", chapterSelect);

			// Get all clients and filter by membership if a filter is applied

			PopulateAttendance(chapterSelect, rehearsalToUpdate);
			return View(rehearsalToUpdate);
		}

		// GET: Rehearsal/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var rehearsal = await _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (rehearsal == null)
			{
				return NotFound();
			}

			return View(rehearsal);
		}

		// POST: Rehearsal/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var rehearsal = await _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Singer)
				.FirstOrDefaultAsync(m => m.ID == id);

			try
			{
				if (rehearsal != null)
				{
					_context.Rehearsals.Remove(rehearsal);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}
			return View(rehearsal);
		}

		private SelectList DirectorSelectList(int? id)
		{
			return new SelectList(_context.Directors
				.OrderBy(d => d.FirstName), "ID", "NameFormatted", id);
		}

		private SelectList ChapterSelectList(int? id)
		{
			return new SelectList(_context.Chapters
				.OrderBy(c => c.Name), "ID", "Name", id);
		}

		private void PopulateDropDown(Rehearsal? rehearsal = null)
		{
			ViewData["DirectorID"] = DirectorSelectList(rehearsal?.DirectorID);
			ViewData["ChapterID"] = ChapterSelectList(rehearsal?.Director?.ChapterID);
		}

		private void PopulateAttendance(int? chapterSelect, Rehearsal rehearsal)
		{
			var singers = _context.Singers.Include(c => c.Chapter).AsQueryable();
			if (chapterSelect.HasValue)
			{
				singers = singers.Where(c => c.ChapterID == chapterSelect.Value);
			}

			// Format available clients with membership information

			var allOptions = singers;
			var currentOptionsHS = new HashSet<int>(rehearsal.RehearsalAttendances.Select(b => b.SingerID));
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
						DisplayText = $"{c.NameFormatted} ({(c.Chapter != null ? c.Chapter.Name : "None")})"
					});
				}
				else
				{
					available.Add(new ListOptionVM
					{
						ID = c.ID,
						DisplayText = $"{c.NameFormatted} ({(c.Chapter != null ? c.Chapter.Name : "None")})"
					});
				}
			}

			ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
			ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
		}

		private void UpdateAttendance(string[] selectedOptions, Rehearsal rehearsalToUpdate)
		{
			if (selectedOptions == null)
			{
				rehearsalToUpdate.RehearsalAttendances = new List<RehearsalAttendance>();
				return;
			}

			var selectedOptionsHS = new HashSet<string>(selectedOptions);
			var currentOptionsHS = new HashSet<int>(rehearsalToUpdate.RehearsalAttendances.Select(b => b.SingerID));
			foreach (var s in _context.Singers)
			{
				if (selectedOptionsHS.Contains(s.ID.ToString()))//it is selected
				{
					if (!currentOptionsHS.Contains(s.ID))//but not currently in the Doctor's collection - Add it!
					{
						rehearsalToUpdate.RehearsalAttendances.Add(new RehearsalAttendance
						{
							SingerID = s.ID,
							RehearsalID = rehearsalToUpdate.ID
						});
					}
				}
				else //not selected
				{
					if (currentOptionsHS.Contains(s.ID))//but is currently in the Doctor's collection - Remove it!
					{
						RehearsalAttendance? specToRemove = rehearsalToUpdate.RehearsalAttendances
							.FirstOrDefault(d => d.SingerID == s.ID);
						if (specToRemove != null)
						{
							_context.Remove(specToRemove);
						}
					}
				}
			}
		}

		public IActionResult GetSingersByChapter(int? chapterSelect)
		{
			// Get all clients and filter by membership type if specified
			var singers = _context.Singers.Include(c => c.Chapter).AsQueryable();
			if (chapterSelect.HasValue)
			{
				singers = singers.Where(c => c.ChapterID == chapterSelect.Value);
			}

			// Format the client list for the response
			var clientList = singers.Select(c => new
			{
				id = c.ID,
				DisplayText = $"{c.NameFormatted} ({(c.Chapter != null ? c.Chapter.Name : "None")})"
			}).ToList();

			return Json(clientList);
		}

		// Rehearsal Calender View
		public PartialViewResult RehearsalCalender()
		{
			var rehearsals = _context.Rehearsals
				.Include(r => r.Director).ThenInclude(d => d.Chapter)
				.OrderBy(r => r.RehearsalDate)
				.ToList();

			return PartialView("_RehearsalCalendar");
		}

		public string GetColor(int chapter)
		{
			switch (chapter)
			{
				case 1:
					return "#8f3796";

				default:
					return "#fff";
			}
		}

		public async Task<IActionResult> GetRehearsals()
		{
			var rehearsals = await _context.Rehearsals
				.Select(r => new
				{
					id = r.ID,
					groupId = r.Director.ChapterID,
					title = r.Director.Chapter.Name,
					start = $"{r.RehearsalDate.ToString("yyyy-MM-dd")}{r.StartTime.ToString("THH:mm:ss")}",
					end = $"{r.RehearsalDate.ToString("yyyy-MM-dd")}{r.EndTime.ToString("THH:mm:ss")}",
					textColor = "black",
					backgroundColor =
					r.Director.ChapterID == 1 ? "#ffadad" :
					r.Director.ChapterID == 2 ? "#ffd6a5" :
					r.Director.ChapterID == 3 ? "#fdffb6" :
					r.Director.ChapterID == 4 ? "#caffbf" :
					r.Director.ChapterID == 5 ? "#a0c4ff" :
					r.Director.ChapterID == 6 ? "#bdb2ff" :
					"#fff",
					url = Url.Action("Details", "Rehearsal", new { id = r.ID })
				})
				.ToListAsync();
			return Json(rehearsals);
		}

		public async Task<IActionResult> RehearsalsSummary(DateTime? startDate, DateTime? endDate)
		{
			startDate ??= new DateTime(2020, 1, 1);  // Default to January 1st, 1st year
			endDate ??= DateTime.Now;  // Default to today's date
			var sumQ = _context.Rehearsals.Include(c => c.RehearsalAttendances)
				 .Include(c => c.Director).ThenInclude(c => c.Chapter)
				 .Where(a => a.RehearsalDate >= startDate && a.RehearsalDate <= endDate)
				 .GroupBy(a => new { a.Director.Chapter.City })
				 .Select(grp => new AttendanceSummaryVM
				 {
					 City = grp.Key.City,
					 Number_Of_Rehearsals = grp.Count(),
					 Average_Attendance = Math.Round(grp.Average(a => a.RehearsalAttendances.Count), 1),
					 Highest_Attendance = grp.Max(a => a.RehearsalAttendances.Count),
					 Lowest_Attendance = grp.Min(a => a.RehearsalAttendances.Count),
					 Total_Attendance = grp.Sum(a => a.RehearsalAttendances.Count)
				 });

			ViewData["StartDate"] = startDate;
			ViewData["EndDate"] = endDate;
			return View(sumQ);
		}

		public IActionResult RehearsalsSummaryReport(DateTime? startDate, DateTime? endDate)
		{
			var sumQ = _context.Rehearsals.Include(c => c.RehearsalAttendances)
				  .Include(c => c.Director).ThenInclude(c => c.Chapter)
				  .Where(a => a.RehearsalDate >= startDate && a.RehearsalDate <= endDate)
				  .GroupBy(a => new { a.Director.Chapter.City })
				  .Select(grp => new AttendanceSummaryVM
				  {
					  City = grp.Key.City,
					  Number_Of_Rehearsals = grp.Count(),
					  Average_Attendance = grp.Average(a => a.RehearsalAttendances.Count),
					  Highest_Attendance = grp.Max(a => a.RehearsalAttendances.Count),
					  Lowest_Attendance = grp.Min(a => a.RehearsalAttendances.Count),
					  Total_Attendance = grp.Sum(a => a.RehearsalAttendances.Count)
				  });
			//How many rows?
			int numRows = sumQ.Count();

			if (numRows > 0) //We have data
			{
				//Create a new spreadsheet from scratch.
				using (ExcelPackage excel = new ExcelPackage())
				{
					string startDate1 = startDate.Value.ToShortDateString();

					string endDate1 = endDate.Value.ToShortDateString();

					var workSheet = excel.Workbook.Worksheets.Add($"RehearsalsSummaryReport from {startDate1} to {endDate1}");

					//Note: Cells[row, column]
					workSheet.Cells[3, 1].LoadFromCollection(sumQ, true);

					//Style column for currency
					workSheet.Column(3).Style.Numberformat.Format = "###,##0.0";
					workSheet.Column(4).Style.Numberformat.Format = "###,##0.0";
					workSheet.Column(5).Style.Numberformat.Format = "###,##0.0";
					workSheet.Column(6).Style.Numberformat.Format = "###,##0.0";

					//Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
					//Make Date and Patient Bold
					workSheet.Cells[4, 1, numRows + 3, 2].Style.Font.Bold = true;

					//Note: these are fine if you are only 'doing' one thing to the range of cells.
					//Otherwise you should USE a range object for efficiency
					using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 1])//
					{
						totalfees.Value = "Totals:";
						totalfees.Style.Font.Bold = true;
					}
					using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 2])//
					{
						totalfees.Formula = "Sum(" + workSheet.Cells[4, 2].Address + ":" + workSheet.Cells[numRows + 3, 2].Address + ")";
						totalfees.Style.Font.Bold = true;
						totalfees.Style.Numberformat.Format = "###,##0";
					}
					using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 6])//
					{
						totalfees.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
						totalfees.Style.Font.Bold = true;
						totalfees.Style.Numberformat.Format = "###,##0.0";
					}

					//Set Style and backgound colour of headings
					using (ExcelRange headings = workSheet.Cells[3, 1, 3, 6])
					{
						headings.Style.Font.Bold = true;
						var fill = headings.Style.Fill;
						fill.PatternType = ExcelFillStyle.Solid;
						fill.BackgroundColor.SetColor(Color.LightBlue);
					}

					//Autofit columns
					workSheet.Cells.AutoFitColumns();
					//Note: You can manually set width of columns as well
					//workSheet.Column(7).Width = 10;

					//Add a title and timestamp at the top of the report
					workSheet.Cells[1, 1].Value = $"Rehearsals Summary Report from {startDate1} to {endDate1}";
					using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 6])
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
					using (ExcelRange Rng = workSheet.Cells[2, 6])
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
						string filename = $"RehearsalsSummaryReport from {startDate1} to {endDate1}.xlsx";
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

		private bool RehearsalExists(int id)
		{
			return _context.Rehearsals.Any(e => e.ID == id);
		}
	}
}