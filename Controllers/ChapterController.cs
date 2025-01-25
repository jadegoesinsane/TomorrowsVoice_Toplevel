using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class ChapterController : ElephantController
    {
        private readonly TVContext _context;

        public ChapterController(TVContext context)
        {
            _context = context;
        }

        // GET: Chapter
        public async Task<IActionResult> Index(string? SearchString, List<int?> ChapterID, int? page, int? pageSizeID, string? ProvinceFilter,

			string? actionButton, string sortDirection = "asc", string sortField = "Chapter")
        {

            string[] sortOptions = new[] {"Chapter","Name" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
			if (Enum.TryParse(ProvinceFilter, out Province selectedDOW))
			{
				ViewBag.DOWSelectList = Province.Ontario.ToSelectList(selectedDOW);
			}
			else
			{
				ViewBag.DOWSelectList = Province.Ontario.ToSelectList(null);
			}

			var chapters =  _context.Chapters
                .AsNoTracking()
                 .AsNoTracking();

			if (!String.IsNullOrEmpty(ProvinceFilter))
			{
				chapters = chapters.Where(p => p.Province == selectedDOW);
				numberFilters++;
			}
			if (!String.IsNullOrEmpty(SearchString))
            {
                chapters = chapters.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper()));
                                      
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
            if (sortField == "Name")
            {
                if (sortDirection == "asc")
                {
                    chapters = chapters
                        .OrderBy(s => s.Name);
                }
                else
                {
                    chapters = chapters
                        .OrderByDescending(s => s.Name);
                        
                }
            }
            else if (sortField == "Chapter")
            {
                if (sortDirection == "asc")
                {
                    chapters = chapters
                        .OrderBy(s => s.Name);
                }
                else
                {
                    chapters = chapters
                        .OrderByDescending(s => s.Name);
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

        // GET: Chapter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // GET: Chapter/Create
        public IActionResult Create()
        {
            Chapter chapter = new Chapter();
            return View(chapter);
        }

        // POST: Chapter/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Address,PostalCode,Province")] Chapter chapter)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(chapter);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { chapter.ID });
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return View(chapter);
        }

        // GET: Chapter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.ID == id);

            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Chapter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            // Get the Chapter to update
            var chapterToUpdate = await _context.Chapters
                .Include(c => c.Directors)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (chapterToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Chapter>(chapterToUpdate, "",
                c => c.Name, c => c.Address, c => c.PostalCode, c => c.Province))
            {
                try
                {
                    await _context.SaveChangesAsync();
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
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }

            return View(chapterToUpdate);
        }

        // GET: Chapter/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Directors)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Directors)
                .FirstOrDefaultAsync(c => c.ID == id);
            try
            {
                if (chapter != null)
                {
                    _context.Chapters.Remove(chapter);
                }

                await _context.SaveChangesAsync();
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
            
            return View(chapter);
        }
        public PartialViewResult ListOfDirectorsDetails(int id)
        {
            var query = from p in _context.Directors
                        where p.ChapterID == id
                        orderby p.LastName, p.FirstName
                        select p;
            return PartialView("_ListOfDirectorsDetails", query.ToList());
        }
       
        public PartialViewResult ListOfSingersDetails(int id)
        {
            var query = from p in _context.Singers
                        where p.ChapterID == id
                        orderby p.LastName, p.FirstName
                        select p;
            return PartialView("_ListOfSingersDetails", query.ToList());
        }
       
        
        private bool ChapterExists(int id)
        {
            return _context.Chapters.Any(e => e.ID == id);
        }
    }
}
