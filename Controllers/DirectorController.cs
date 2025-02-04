using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class DirectorController : ElephantController
	{
		private readonly TVContext _context;

		public DirectorController(TVContext context, IToastNotification toastNotification) : base(toastNotification)
		{
			_context = context;
		}

		// GET: Director
		public async Task<IActionResult> Index(string? SearchString, List<int?> ChapterID, int? page, int? pageSizeID,
			string? actionButton, string? StatusFilter, string sortDirection = "asc", string sortField = "Director")
		{
			string[] sortOptions = new[] { "Director", "Chapter" };

			//Count the number of filters applied - start by assuming no filters
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			// Select list for statuses
			if (Enum.TryParse(StatusFilter, out Status selectedStatus))
			{
				ViewBag.StatusSelectList = Status.Active.ToSelectList(selectedStatus);
			}
			else
			{
				ViewBag.StatusSelectList = Status.Active.ToSelectList(null);
			}

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
			//  filter by status
			if (!String.IsNullOrEmpty(StatusFilter))
			{
				directors = directors.Where(p => p.Status == selectedStatus);

				// filter out archived singers if the user does not specifically select "archived"
				if (selectedStatus != Status.Archived)
				{
					directors = directors.Where(d => d.Status != Status.Archived);
				}
				numberFilters++;
			}
			// filter out singers even if status filter has not been set
			else
			{
				directors = directors.Where(d => d.Status != Status.Archived);
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
			PopulateDropDownLists();
			//Handle Paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<Director>.CreateAsync(directors.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// GET: Director/Details/5
		public async Task<IActionResult> Details(int? id, int? chapterID)
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
			ViewBag.ChapterID = chapterID;
			return View(director);
		}

		// GET: Director/Create
		public IActionResult Create()
		{
			Director director = new Director();
			PopulateDropDownLists(director);
			return View(director);
		}

		// POST: Director/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Email,Phone,ChapterID,Status")] Director director,
			List<IFormFile> theFiles)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await AddDocumentsAsync(director, theFiles);
					_context.Add(director);
					await _context.SaveChangesAsync();
					return RedirectToAction("Details", new { director.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Email"))
				{
					ModelState.AddModelError("Email", "Instructor email address must be unique. Please enter a unique director email address.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			PopulateDropDownLists(director);
			return View(director);
		}

		// GET: Director/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var director = await _context.Directors
				.Include(d => d.VulnerableSectorChecks)
				.FirstOrDefaultAsync(d => d.ID == id);

			if (director == null)
			{
				return NotFound();
			}
			PopulateDropDownLists(director);
			return View(director);
		}

		// POST: Director/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Byte[] RowVersion, List<IFormFile> theFiles)
		{
			var directorToUpdate = await _context.Directors
				.Include(d => d.Chapter)
				.Include(d => d.Rehearsals)
				.Include(d => d.VulnerableSectorChecks)
				.FirstOrDefaultAsync(d => d.ID == id);

			if (directorToUpdate == null)
			{
				return NotFound();
			}

			//Put the original RowVersion value in the OriginalValues collection for the entity
			_context.Entry(directorToUpdate).Property("RowVersion").OriginalValue = RowVersion;

			if (await TryUpdateModelAsync<Director>(directorToUpdate, "",
				d => d.FirstName, d => d.MiddleName, d => d.LastName, d => d.Email, d => d.Phone, r => r.Status, r => r.ChapterID))
			{
				try
				{
					await AddDocumentsAsync(directorToUpdate, theFiles);
					await _context.SaveChangesAsync();
					return RedirectToAction("Details", new { directorToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectorExists(directorToUpdate.ID))
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
					if (message.Contains("UNIQUE") && message.Contains("Email"))
					{
						ModelState.AddModelError("Email", "Instructor email address must be unique. Please enter a unique director email address.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}
			PopulateDropDownLists(directorToUpdate);
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
				.Include(d => d.VulnerableSectorChecks)
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
				.Include(d => d.VulnerableSectorChecks)
				.FirstOrDefaultAsync(d => d.ID == id);
			try
			{
				if (director != null)
				{
					//_context.Directors.Remove(director);

					// Archive a director instead of deleting them
					director.Status = Status.Archived;
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

		// For Adding Chapters
		private SelectList ChapterSelectList(int? selectedId)
		{
			return new SelectList(_context
				.Chapters
				.OrderBy(c => c.Name), "ID", "Name", selectedId);
		}

		public void PopulateDropDownLists(Director? director = null)
		{
			ViewData["ChapterID"] = ChapterSelectList(director?.ChapterID);
		}

		//Partial View for Directors Rehearsal View
		public PartialViewResult DirectorsRehearsalList(int id)
		{
			var rehearsals = _context.Rehearsals
				.Where(r => r.DirectorID == id)
				.OrderBy(r => r.RehearsalDate)
				.ToList();
			ViewBag.DirectorID = id;
			return PartialView("_DirectorsRehearsalList", rehearsals);
		}

		public PartialViewResult ListOfDocumentsDetails(int id)
		{
			var query = from d in _context.DirectorDocuments
						where d.DirectorID == id
						orderby d.FileName
						select d;
			ViewBag.DirectorID = id;
			return PartialView("_ListOfDocuments", query.ToList());
		}

		public async Task<FileContentResult> Download(int id)
		{
			var theFile = await _context.UploadedFiles
				.Include(d => d.FileContent)
				.Where(f => f.ID == id)
				.FirstOrDefaultAsync();

			if (theFile?.FileContent?.Content == null || theFile.MimeType == null)
			{
				return new FileContentResult(Array.Empty<byte>(), "application/octet-stream");
			}

			return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
		}

		private async Task AddDocumentsAsync(Director director, List<IFormFile> theFiles)
		{
			foreach (var f in theFiles)
			{
				if (f != null)
				{
					string mimeType = f.ContentType;
					string fileName = Path.GetFileName(f.FileName);
					long fileLength = f.Length;

					if (!(fileName == "" || fileLength == 0))
					{
						DirectorDocument d = new DirectorDocument();
						using (var memoryStream = new MemoryStream())
						{
							await f.CopyToAsync(memoryStream);
							d.FileContent.Content = memoryStream.ToArray();
						}
						d.MimeType = mimeType;
						d.FileName = fileName;
						director.VulnerableSectorChecks.Add(d);
					};
				}
			}
		}

		public JsonResult GetDirectorData()
		{
			var data = new Director
			{
				FirstName = "Bob",
				LastName = "Jeremy",
				Email = "bjeremy@gmail.com",
				Phone = "9051012233",
				ChapterID = 7
			};
			return Json(data);
		}

		private bool DirectorExists(int id)
		{
			return _context.Directors.Any(e => e.ID == id);
		}
	}
}