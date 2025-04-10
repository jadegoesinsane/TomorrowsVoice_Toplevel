using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class ChapterController : ElephantController
	{
		private readonly TVContext _context;

		public ChapterController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		[Authorize(Roles ="Admin, Director")]
		// GET: Chapter
		public async Task<IActionResult> Index(string? SearchString, List<int?> ChapterID, int? page, int? pageSizeID, string? StatusFilter, string? ProvinceFilter,

			string? actionButton, string sortDirection = "asc", string sortField = "Chapter")
		{
			string[] sortOptions = new[] { "City" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			Enum.TryParse(StatusFilter, out Status selectedStatus);

			var statusList = Enum.GetValues(typeof(Status))
						 .Cast<Status>()
						 .Where(s => s == Status.Active || s == Status.Archived)
						 .ToList();

			ViewBag.StatusSelectList = new SelectList(statusList);
			if (Enum.TryParse(ProvinceFilter, out Province selectedDOW))
			{
				ViewBag.DOWSelectList = Province.Ontario.ToSelectList(selectedDOW);
			}
			else
			{
				ViewBag.DOWSelectList = Province.Ontario.ToSelectList(null);
			}

			var chapters = _context.Chapters
				.Include(c => c.Singers)
				.Include(c => c.Directors)
				.Include(c => c.City)

				.AsNoTracking();
			if (!String.IsNullOrEmpty(StatusFilter))
			{
				chapters = chapters.Where(p => p.Status == selectedStatus);

				// filter out archived singers if the user does not specifically select "archived"
				if (selectedStatus != Status.Archived)
				{
					chapters = chapters.Where(d => d.Status != Status.Archived);
				}
				numberFilters++;
			}
			// filter out singers even if status filter has not been set
			else
			{
				chapters = chapters.Where(d => d.Status != Status.Archived);
			}
			if (!String.IsNullOrEmpty(ProvinceFilter))
			{
				chapters = chapters.Where(c => c.City.Province == selectedDOW);
				numberFilters++;
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				chapters = chapters.Where(c => c.City.Name.ToUpper().Contains(SearchString.ToUpper()));
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
			if (sortField == "City")
			{
				if (sortDirection == "asc")
				{
					chapters = chapters
						.OrderBy(s => s.City.Name);
				}
				else
				{
					chapters = chapters
						.OrderByDescending(s => s.City.Name);
				}
			}

			//Set sort for next time
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;
			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Chapter>.CreateAsync(chapters.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

        [Authorize(Roles = "Admin, Director")]
        // GET: Chapter/Details/5
        public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var chapter = await _context.Chapters
				.Include(c => c.City)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);
			if (chapter == null)
			{
				return NotFound();
			}
			PopulateDropDownLists(chapter);
			return View(chapter);
		}
        
		[Authorize(Roles = "Admin")]
        // GET: Chapter/Create
        public IActionResult Create()
		{
			Chapter chapter = new Chapter();
			PopulateDropDownLists(chapter);
			return View(chapter);
		}

		// POST: Chapter/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name,Address,PostalCode,Province,CityID")] Chapter chapter)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(chapter);
					await _context.SaveChangesAsync();
					var city = await _context.Cities.FirstOrDefaultAsync(c => c.ID == chapter.CityID);
					AddSuccessToast(city.Name);
					return RedirectToAction("Details", new { chapter.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE constraint failed") && message.Contains("Chapters.Name"))
				{
					ModelState.AddModelError("Name", "Chapter name must be unique. Please enter a unique chapter name.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			PopulateDropDownLists(chapter);
			return View(chapter);
		}

        [Authorize(Roles = "Admin")]
        // GET: Chapter/Edit/5
        public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var chapter = await _context.Chapters
				.Include(c => c.City)
				.FirstOrDefaultAsync(c => c.ID == id);

			if (chapter == null)
			{
				return NotFound();
			}
			PopulateDropDownLists(chapter);
			return View(chapter);
		}

		// POST: Chapter/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
		{
			// Get the Chapter to update
			var chapterToUpdate = await _context.Chapters
				.Include(c => c.Directors)
				.Include(c => c.City)
				.FirstOrDefaultAsync(c => c.ID == id);

			if (chapterToUpdate == null)
			{
				return NotFound();
			}

			//Put the original RowVersion value in the OriginalValues collection for the entity
			_context.Entry(chapterToUpdate).Property("RowVersion").OriginalValue = RowVersion;

			//Try updating it with the values posted
			if (await TryUpdateModelAsync<Chapter>(chapterToUpdate, "",
				c => c.CityID, c => c.PostalCode))
			{
				try
				{
					await _context.SaveChangesAsync();
					AddSuccessToast(chapterToUpdate.City.Name);
					//return RedirectToAction(nameof(Index));
					//Instead of going back to the Index, why not show the revised
					//version in full detail?
					return RedirectToAction("Details", new { chapterToUpdate.ID });
				}
				catch (RetryLimitExceededException /* dex */)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ChapterExists(chapterToUpdate.ID))
					{
						return NotFound();
					}
					else
					{
						ModelState.AddModelError(string.Empty, "The record you attempted to edit "
							+ "was modified by another user. Please go back and refresh.");
					}
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE constraint failed") && message.Contains("Chapters.Name"))
					{
						ModelState.AddModelError("Name", "Chapter name must be unique. Please enter a unique chapter name.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}

			return View(chapterToUpdate);
		}


        [Authorize(Roles = "Admin")]
        // GET: Chapter/Delete/5
        public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var chapter = await _context.Chapters
				.Include(c => c.Directors)
				.Include(c => c.City)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);
			if (chapter == null)
			{
				return NotFound();
			}

			return View(chapter);
		}

		// POST: Chapter/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var chapter = await _context.Chapters
				.Include(c => c.Directors).Include(c => c.City).Include(c => c.Singers).Include(c => c.Rehearsals)
				.FirstOrDefaultAsync(c => c.ID == id);
			try
			{
				if (chapter != null)
				{
					//_context.Chapters.Remove(chapter);
					// Archive a chatper instead of deleting it

					chapter.Status = Status.Archived;

					foreach (var rehearsal in chapter.Rehearsals)
					{
						rehearsal.Status = Status.Archived;
					}
					foreach (var a in chapter.Directors)
					{
						a.Status = Status.Archived;
					}
					foreach (var a in chapter.Singers)
					{
						a.Status = Status.Archived;
					}
				}

				await _context.SaveChangesAsync();
				AddSuccessToast(chapter.City.Name);
				var returnUrl = ViewData["returnURL"]?.ToString();
				if (string.IsNullOrEmpty(returnUrl))
				{
					return RedirectToAction(nameof(Index));
				}
				return Redirect(returnUrl);
			}
			catch (DbUpdateException dex)
			{
				if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
				{
					ModelState.AddModelError("", "Unable to Delete Chapter. Remember, you cannot delete a Chapter.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			catch (InvalidOperationException)
			{
				ModelState.AddModelError("", $"Unable to delete a chapter that has active director or avtive singers associated with it.");
			}

			return View(chapter);
		}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Recover(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var chapter = await _context.Chapters
				.Include(c => c.Directors)
				.Include(c => c.City)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);
			if (chapter == null)
			{
				return NotFound();
			}

			return View(chapter);
		}

		// POST: Chapter/Delete/5
		[HttpPost, ActionName("Recover")]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecoverConfirmed(int id)
		{
			var chapter = await _context.Chapters
				.Include(c => c.Directors).Include(c => c.City).Include(c => c.Singers).Include(c => c.Rehearsals)
				.FirstOrDefaultAsync(c => c.ID == id);
			try
			{
				if (chapter != null)
				{
					//_context.Chapters.Remove(chapter);
					// Archive a chatper instead of deleting it

					chapter.Status = Status.Active;

					foreach (var rehearsal in chapter.Rehearsals)
					{
						rehearsal.Status = Status.Active;
					}
					foreach (var a in chapter.Directors)
					{
						a.Status = Status.Active;
					}
					foreach (var a in chapter.Singers)
					{
						a.Status = Status.Active;
					}
				}

				await _context.SaveChangesAsync();
				AddSuccessToast(chapter.City.Name);
				var returnUrl = ViewData["returnURL"]?.ToString();
				if (string.IsNullOrEmpty(returnUrl))
				{
					return RedirectToAction(nameof(Index));
				}
				return Redirect(returnUrl);
			}
			catch (DbUpdateException dex)
			{
				if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
				{
					ModelState.AddModelError("", "Unable to Delete Chapter. Remember, you cannot delete a Chapter.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			catch (InvalidOperationException)
			{
				ModelState.AddModelError("", $"Unable to delete a chapter that has active director or avtive singers associated with it.");
			}

			return View(chapter);
		}

		public void PopulateDropDownLists(Chapter? chapter = null)
		{
			ViewData["CityID"] = CitySelectList(chapter?.CityID, Status.Active);
		}

		public PartialViewResult ListOfDirectorsDetails(int id)
		{
			var query = from p in _context.Directors
						where (p.ChapterID == id && p.Status == Status.Active)
						orderby p.LastName, p.FirstName
						select p;
			ViewBag.ChapterID = id;
			return PartialView("_ListOfDirectorsDetails", query.ToList());
		}

		public PartialViewResult ListOfSingersDetails(int id)
		{
			var query = from p in _context.Singers
						where (p.ChapterID == id && p.Status == Status.Active)
						orderby p.LastName, p.FirstName
						select p;
			ViewBag.ChapterID = id;
			return PartialView("_ListOfSingersDetails", query.ToList());
		}

		public JsonResult GetChapterData()
		{
			var data = new Chapter
			{
				//Name = "Welland",
				//Province = Province.Ontario,
				Address = "100 Niagara College Blvd",
				PostalCode = "L3C 7L3"
			};
			return Json(data);
		}

		private bool ChapterExists(int id)
		{
			return _context.Chapters.Any(e => e.ID == id);
		}
	}
}