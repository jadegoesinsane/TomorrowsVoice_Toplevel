using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class SingerController : ElephantController
	{
		private readonly TVContext _context;

		public SingerController(TVContext context)
		{
			_context = context;
		}

		// GET: Singer
		public async Task<IActionResult> Index(string? SearchString, int? ChapterID, int? page, int? pageSizeID,
			string? actionButton, string sortDirection = "asc", string sortField = "Singer")
		{
			// Sort Options
			string[] sortOptions = new[] { "Singer", "Chapter" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			PopulateDropDownLists();

			var singers = _context.Singers
				.Include(s => s.Chapter)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking();

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
				if (sortDirection == "asc")
				{
					singers = singers
						.OrderBy(s => s.LastName)
						.ThenBy(s => s.FirstName);
				}
				else
				{
					singers = singers
						.OrderByDescending(s => s.LastName)
						.ThenBy(s => s.FirstName);
				}
			}
			else if (sortField == "Chapter")
			{
				if (sortDirection == "asc")
				{
					singers = singers
						.OrderBy(s => s.Chapter.Name);
				}
				else
				{
					singers = singers
						.OrderByDescending(s => s.Chapter.Name);
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
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var singer = await _context.Singers
			   .Include(s => s.Chapter)
				.Include(s => s.RehearsalAttendances).ThenInclude(ra => ra.Rehearsal)
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ID == id);
			if (singer == null)
			{
				return NotFound();
			}

			return View(singer);
		}

		// GET: Singer/Create
		public IActionResult Create()
		{
			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
			return View();
		}

		// POST: Singer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,ContactName,Phone,Note,ChapterID,IsActive")] Singer singer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(singer);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
			}

			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singer.ChapterID);
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
				.Include(r => r.Chapter)
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (singer == null)
			{
				return NotFound();
			}
			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singer.ChapterID);
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
			   .Include(r => r.Chapter)
			   .Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
			   .FirstOrDefaultAsync(m => m.ID == id);

			if (singerToUpdate == null)
			{
				return NotFound();
			}

			// Try updating with posted values
			if (await TryUpdateModelAsync<Singer>(singerToUpdate,
					"",
					r => r.FirstName,
					r => r.LastName,
					r => r.ContactName,
					r => r.Phone,
					r => r.Note,
					r => r.ContactName,
					r => r.ChapterID,
					r => r.IsActive))
			{
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction("Details", new { singerToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!SingerExists(singerToUpdate.ID))
					{
						return NotFound();
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Please Try Again.");
					}
				}
			}

			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name", singerToUpdate.ChapterID);
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
				.Include(r => r.RehearsalAttendances).ThenInclude(r => r.Rehearsal)
				.FirstOrDefaultAsync(m => m.ID == id);
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
					_context.Singers.Remove(singer);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return View(singer);
		}

		private SelectList ChapterSelectList(int? selectedId)
		{
			return new SelectList(_context.Chapters
				.OrderBy(c => c.Name), "ID", "Name", selectedId);
		}

		private void PopulateDropDownLists(Singer? singer = null)
		{
			ViewData["ChapterID"] = ChapterSelectList(singer?.ChapterID);
		}

		private bool SingerExists(int id)
		{
			return _context.Singers.Any(e => e.ID == id);
		}
	}
}