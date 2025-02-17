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
using TomorrowsVoice_Toplevel.ViewModels;

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

            Event abc = new Event();
            PopulateAssignedEnrollmentData(abc);


          
            return View();
        }

		// POST: Event/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,StartDate,EndDate,Descripion,Location,Status")] Event @event,string[] selectedOptions)
		{
            try
            {

                UpdateEnrollments(selectedOptions, @event);
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

            PopulateAssignedEnrollmentData(@event);
            return View(@event);

           
		}

		// GET: Event/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			

            var @event = await _context.Events
              .Include(g => g.CityEvents).ThenInclude(e => e.City)
              .FirstOrDefaultAsync(m => m.ID == id);
            if (@event == null)
			{
				return NotFound();
			}

            PopulateAssignedEnrollmentData(@event);
            return View(@event);
		}

		// POST: Event/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {

            var @eventToUpdate = await _context.Events
              .Include(g => g.CityEvents).ThenInclude(e => e.City)
              .FirstOrDefaultAsync(m => m.ID == id);

            if (@eventToUpdate == null)
            {
                return NotFound();
            }
            UpdateEnrollments(selectedOptions, @eventToUpdate);
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
            PopulateAssignedEnrollmentData(@eventToUpdate);

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
        private void PopulateAssignedEnrollmentData(Event abc)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Cities;
            var currentOptionsHS = new HashSet<int>(abc.CityEvents.Select(b => b.CityID));
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
                        DisplayText = c.Name
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = c.ID,
                        DisplayText = c.Name
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateEnrollments(string[] selectedOptions, Event abcToUpdate)
        {
            if (selectedOptions == null)
            {
                abcToUpdate.CityEvents = new List<CityEvent>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(abcToUpdate.CityEvents.Select(b => b.CityID));
            foreach (var c in _context.Volunteers)
            {
                if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
                    {
                        abcToUpdate.CityEvents.Add(new CityEvent
                        {
                            CityID = c.ID,
                            EventID = abcToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
                    {
                        CityEvent? enrollmentToRemove = abcToUpdate.CityEvents
                            .FirstOrDefault(d => d.CityID == c.ID);
                        if (enrollmentToRemove != null)
                        {
                            _context.Remove(enrollmentToRemove);
                        }
                    }
                }
            }
        }
            private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.ID == id);
		}
	}
}