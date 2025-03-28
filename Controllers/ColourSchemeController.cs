using System;
using System.Collections.Generic;
using System.Linq;
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
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ColourSchemeController : ElephantController
	{
		private readonly TVContext _context;

		public ColourSchemeController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: ColourScheme
		public async Task<IActionResult> Index()
		{
			return View(await _context.ColourSchemes.ToListAsync());
		}

		// GET: ColourScheme/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var colourScheme = await _context.ColourSchemes
				.FirstOrDefaultAsync(m => m.ID == id);
			if (colourScheme == null)
			{
				return NotFound();
			}
			GetRatingVB(colourScheme);
			return View(colourScheme);
		}

		// GET: ColourScheme/Create
		public IActionResult Create()
		{
			ColourScheme colourScheme = _context.ColourSchemes.FirstOrDefault();
			GetRatingVB(colourScheme);
			return View(colourScheme);
		}

		// POST: ColourScheme/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,BackgroundColour,TextColour,BorderColour")] ColourScheme colourScheme)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(colourScheme);
					await _context.SaveChangesAsync();
					AddSuccessToast(colourScheme.Name);
					return RedirectToAction("Details", new { colourScheme.ID });
				}
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("Name"))
				{
					ModelState.AddModelError("Name", "Colour Scheme with this name already exists!");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}
			GetRatingVB(colourScheme);
			return View(colourScheme);
		}

		// GET: ColourScheme/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var colourScheme = await _context.ColourSchemes.FindAsync(id);
			if (colourScheme == null)
			{
				return NotFound();
			}
			GetRatingVB(colourScheme);
			return View(colourScheme);
		}

		// POST: ColourScheme/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var colourToUpdate = await _context.ColourSchemes.FirstOrDefaultAsync(c => c.ID == id);
			if (colourToUpdate == null)
			{
				return NotFound();
			}
			if (await TryUpdateModelAsync<ColourScheme>(colourToUpdate,
					"",
					c => c.Name,
					c => c.BackgroundColour,
					c => c.TextColour,
					c => c.BorderColour))
			{
				try
				{
					_context.Update(colourToUpdate);
					await _context.SaveChangesAsync();
					AddSuccessToast(colourToUpdate.Name);
					return RedirectToAction("Details", new { colourToUpdate.ID });
				}
				catch (RetryLimitExceededException)
				{
					ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE") && message.Contains("Name"))
					{
						ModelState.AddModelError("Name", "Colour Scheme with this name already exists!");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}
			}
			GetRatingVB(colourToUpdate);
			return View(colourToUpdate);
		}

		// GET: ColourScheme/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var colourScheme = await _context.ColourSchemes
				.FirstOrDefaultAsync(m => m.ID == id);
			if (colourScheme == null)
			{
				return NotFound();
			}
			GetRatingVB(colourScheme);
			return View(colourScheme);
		}

		// POST: ColourScheme/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var colourScheme = await _context.ColourSchemes.FindAsync(id);
			if (colourScheme != null)
			{
				// Make sure any events & shifts using that colour get their colour defaulted
				var defaultScheme = _context.ColourSchemes.FirstOrDefault();
				if (defaultScheme != null)
				{
					var eventsToUpdate = _context.Events.Where(e => e.ColourID == colourScheme.ID).ToList();
					foreach (var e in eventsToUpdate)
						e.ColourID = defaultScheme.ID;
					var shiftsToUpdate = _context.Shifts.Where(s => s.ColourID == colourScheme.ID).ToList();
					foreach (var s in shiftsToUpdate)
						s.ColourID = defaultScheme.ID;
				}

				_context.ColourSchemes.Remove(colourScheme);
                AddSuccessToast(colourScheme.Name);
            }

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public IActionResult GetRating([FromBody] ColourScheme colourScheme)
		{
			if (colourScheme == null)
			{
				return BadRequest();
			}

			string normalAA = colourScheme.GetRating("AA");
			string normalAAA = colourScheme.GetRating("AAA");
			string bigAA = colourScheme.GetRating("AA", null, true);
			string bigAAA = colourScheme.GetRating("AAA", null, true);

			List<string> tests = new List<string> { normalAA, normalAAA, bigAA, bigAAA };

			double ratio = Math.Round(colourScheme.GetContrastRatio(), 2);

			string status = "FAIL";
			if (tests.All(t => t == "PASS")) status = "PASS";
			else if (tests.All(t => t == "FAIL")) status = "FAIL";
			else status = "MAYBE";

			return Json(new
			{
				normalAA,
				normalAAA,
				bigAA,
				bigAAA,
				ratio,
				status
			});
		}

		public void GetRatingVB(ColourScheme? cs)
		{
			if (cs != null)
			{
				ViewBag.AA = cs.GetRating("AA");
				ViewBag.AAA = cs.GetRating("AAA");
				ViewBag.BigAA = cs.GetRating("AA", null, true);
				ViewBag.BigAAA = cs.GetRating("AAA", null, true);
			}
		}

		private bool ColourSchemeExists(int id)
		{
			return _context.ColourSchemes.Any(e => e.ID == id);
		}
	}
}