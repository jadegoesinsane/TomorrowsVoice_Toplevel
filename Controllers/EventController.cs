using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class EventController : ElephantController
	{
		private readonly TVContext _context;

		public EventController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: Event
		public async Task<IActionResult> Index()
		{
			return View(await _context.Events.ToListAsync());
		}

		// GET: Event/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		// GET: Event/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Event/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,StartDate,EndDate,Descripion,Location,Status")] Event @event)
		{
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    AddSuccessToast(@event.Name);
                    //_toastNotification.AddSuccessToastMessage($"{singer.NameFormatted} was successfully created.");
                    return RedirectToAction("Details", new { @event.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("@event.????????"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, " +
                        "you cannot have duplicate Name and Email.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }


            return View(@event);

           
		}

		// GET: Event/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events.FindAsync(id);
			if (@event == null)
			{
				return NotFound();
			}
			return View(@event);
		}

		// POST: Event/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
            var @eventToUpdate = await _context.Events

               .FirstOrDefaultAsync(m => m.ID == id);

            if (@eventToUpdate == null)
            {
                return NotFound();
            }

            // Try updating with posted values
            if (await TryUpdateModelAsync<Event>(@eventToUpdate,
                    "",
					   r => r.Name,
					   r => r.StartDate,
					 r => r.EndDate,
					 r => r.Location,
                    r => r.Descripion,
                    r => r.Status))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage($"{@eventToUpdate.Name} was successfully updated.");
                    return RedirectToAction("Details", new { @eventToUpdate.ID });
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


            return View(@eventToUpdate);
        }

		// GET: Event/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @event = await _context.Events
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@event == null)
			{
				return NotFound();
			}

			return View(@event);
		}

		// POST: Event/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
            var @event = await _context.Events

               .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (@event != null)
                {
                    //_context.Singers.Remove(singer);

                    // Here we are archiving a singer instead of deleting them
                    @event.Status = Status.Archived;
                    await _context.SaveChangesAsync();
                    AddSuccessToast(@event.Name);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Please try again.");
            }

            return View(@event);
        }

		private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.ID == id);
		}
	}
}