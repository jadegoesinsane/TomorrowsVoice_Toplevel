using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Controllers
{
	public class EventController : ElephantController
	{
		private readonly TVContext _context;

		public EventController(TVContext context, IToastNotification toastNotification) : base(toastNotification)
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
			if (ModelState.IsValid)
			{
				_context.Add(@event);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
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
		public async Task<IActionResult> Edit(int id, [Bind("ID,Name,StartDate,EndDate,Descripion,Location,Status")] Event @event)
		{
			if (id != @event.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(@event);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!EventExists(@event.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(@event);
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
			var @event = await _context.Events.FindAsync(id);
			if (@event != null)
			{
				_context.Events.Remove(@event);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.ID == id);
		}
	}
}