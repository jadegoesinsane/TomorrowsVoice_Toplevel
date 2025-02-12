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
	public class VolunteerController : ElephantController
	{
		private readonly TVContext _context;

		public VolunteerController(TVContext context, IToastNotification toastNotification) : base(toastNotification)
		{
			_context = context;
		}

		// GET: Volunteer
		public async Task<IActionResult> Index()
		{
			return View(await _context.Volunteers.ToListAsync());
		}

		// GET: Volunteer/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers
				.FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}

			return View(volunteer);
		}

		// GET: Volunteer/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Volunteer/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			if (ModelState.IsValid)
			{
				_context.Add(volunteer);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(volunteer);
		}

		// GET: Volunteer/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers.FindAsync(id);
			if (volunteer == null)
			{
				return NotFound();
			}
			return View(volunteer);
		}

		// POST: Volunteer/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
		{
			if (id != volunteer.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(volunteer);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!VolunteerExists(volunteer.ID))
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
			return View(volunteer);
		}

		// GET: Volunteer/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var volunteer = await _context.Volunteers
				.FirstOrDefaultAsync(m => m.ID == id);
			if (volunteer == null)
			{
				return NotFound();
			}

			return View(volunteer);
		}

		// POST: Volunteer/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var volunteer = await _context.Volunteers.FindAsync(id);
			if (volunteer != null)
			{
				_context.Volunteers.Remove(volunteer);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool VolunteerExists(int id)
		{
			return _context.Volunteers.Any(e => e.ID == id);
		}
	}
}