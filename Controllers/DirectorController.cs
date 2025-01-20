using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class DirectorController : Controller
    {
        private readonly TVContext _context;

        public DirectorController(TVContext context)
        {
            _context = context;
        }

        // GET: Director
        public async Task<IActionResult> Index()
        {
            var tVContext = _context.Director
                .Include(d => d.Chapter)
                .AsNoTracking();

            return View(await tVContext.ToListAsync());
        }

        // GET: Director/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Director
                .Include(d => d.Chapter)
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
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "Name");
            return View();
        }

        // POST: Director/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Email,Phone,ChapterID,IsActive")] Director director)
        {
            try { 
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
                        "you cannot have duplicate Email addresses for Instructors.");
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

            var director = await _context.Director.FindAsync(id);
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
                .FirstOrDefaultAsync(d => d.ID == id);

            if (directorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Director>(directorToUpdate, "", d => d.FirstName, d => d.LastName, d => d.Email, d => d.Phone,
                    r => r.IsActive))
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
                            "you cannot have duplicate Email addresses for Instructors.");
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

            var director = await _context.Director
                .Include(d => d.Chapter)
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
                    ModelState.AddModelError("", "Unable to Delete Instructor. Remember, you cannot delete a Instructor that teaches Group Classes.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(director);
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
            return _context.Director.Any(e => e.ID == id);
        }
    }
}
