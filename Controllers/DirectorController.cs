using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class DirectorController : ElephantController
	{
		private readonly TVContext _context;

		public DirectorController(TVContext context)
		{
			_context = context;
		}

		// GET: Director
		public async Task<IActionResult> Index(string? SearchString, List<int?> ChapterID, int? page, int? pageSizeID,
			string? actionButton, string sortDirection = "asc", string sortField = "Director")
		{
			string[] sortOptions = new[] { "Director", "Chapter" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			var directors = _context.Directors
				.Include(d => d.Chapter)
				.Include(d => d.Rehearsals)
				.AsNoTracking();

			if (ChapterID.Any(c => c.HasValue))
			{
				directors = directors.Where(s => ChapterID.Contains(s.ChapterID));
				foreach (int? id in ChapterID)
					numberFilters++;
			}
			if (!String.IsNullOrEmpty(SearchString))
			{
				directors = directors.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
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
			if (sortField == "Director")
			{
				if (sortDirection == "asc")
				{
					directors = directors
						.OrderBy(s => s.LastName)
						.ThenBy(s => s.FirstName);
				}
				else
				{
					directors = directors
						.OrderByDescending(s => s.LastName)
						.ThenBy(s => s.FirstName);
				}
			}
			else if (sortField == "Chapter")
			{
				if (sortDirection == "asc")
				{
					directors = directors
						.OrderBy(s => s.Chapter.Name);
				}
				else
				{
					directors = directors
						.OrderByDescending(s => s.Chapter.Name);
				}
			}

			//Set sort for next time
			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;
			ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Director>.CreateAsync(directors.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// GET: Director/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var director = await _context.Directors
				.Include(d => d.Chapter)
				.Include(d => d.Rehearsals)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);
			if (director == null)
			{
				return NotFound();
			}

			return View(director);
		}

		// GET: Director/Create
		public IActionResult Create()
		{
            Director director = new Director();
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
			return View(director);
		}

		// POST: Director/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Email,Phone,ChapterID,Status")] Director director)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(director);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Email"))
				{
					ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
						"you cannot have duplicate Email addresses for Directors.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			ViewData["ChapterID"] = ChapterSelectList(director.ChapterID);
			return View(director);
		}

		// GET: Director/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var director = await _context.Directors.FindAsync(id);
			if (director == null)
			{
				return NotFound();
			}
			ViewData["ChapterID"] = ChapterSelectList(director.ChapterID);
			return View(director);
		}

		// POST: Director/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var directorToUpdate = await _context.Directors
				.Include(d => d.Chapter)
				.Include(d => d.Rehearsals)
				.FirstOrDefaultAsync(d => d.ID == id);

			if (directorToUpdate == null)
			{
				return NotFound();
			}

			if (await TryUpdateModelAsync<Director>(directorToUpdate, "", d => d.FirstName, d => d.MiddleName, d => d.LastName, d => d.Email, d => d.Phone,
					r => r.Status, r=>r.ChapterID))
			{
				try
				{
					await _context.SaveChangesAsync();
					var returnUrl = ViewData["returnURL"]?.ToString();

					if (string.IsNullOrEmpty(returnUrl))
					{
						return RedirectToAction(nameof(Index));
					}
					return Redirect(returnUrl);
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectorExists(directorToUpdate.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE") && message.Contains("Email"))
					{
						ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
							"you cannot have duplicate Email addresses for Directors.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}
			return View(directorToUpdate);
		}

		// GET: Director/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var director = await _context.Directors
				.Include(d => d.Chapter)
				.Include(d => d.Rehearsals)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);
			if (director == null)
			{
				return NotFound();
			}
			return View(director);
		}

		// POST: Director/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var director = await _context.Directors
				.FirstOrDefaultAsync(d => d.ID == id);
			try
			{
				if (director != null)
				{
					_context.Directors.Remove(director);
				}

				await _context.SaveChangesAsync();
				var returnUrl = ViewData["returnUrl"]?.ToString();
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
					ModelState.AddModelError("", "Unable to Delete Director. Remember, you cannot delete a Director that has rehearsals.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			return View(director);
		}

        //Partial View for Directors Rehearsal View
        public PartialViewResult DirectorsRehearsalList(int id)
        {
            var rehearsals = _context.Rehearsals
                .Where(r => r.DirectorID == id)
                .OrderBy(r => r.RehearsalDate)
                .ToList();

            return PartialView("_DirectorsRehearsalList", rehearsals);
        }


        // For Adding Chapters
        private SelectList ChapterSelectList(int? id)
		{
			var cQuery = from c in _context.Chapters
						 orderby c.Name
						 select c;
			return new SelectList(cQuery, "ID", "Name", id);
		}

		private bool DirectorExists(int id)
		{
			return _context.Directors.Any(e => e.ID == id);
		}
	}
}