using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin")]
	public class GroupController : ElephantController
	{
		private readonly TVContext _context;

		public GroupController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: Group
		public async Task<IActionResult> Index()
		{
			var groups = await _context.Groups
				.Include(g => g.VolunteerGroups)
					.ThenInclude(vg => vg.Volunteer)
				.AsNoTracking()
				.ToListAsync();
			return View(groups);
		}

		// GET: Group/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups
				.Include(g => g.VolunteerGroups)
				.ThenInclude(vg => vg.Volunteer)
				.AsNoTracking()
				.FirstOrDefaultAsync(g => g.ID == id);

			if (@group == null)
			{
				return NotFound();
			}

			return View(@group);
		}

		// GET: Group/Create
		public IActionResult Create()
		{
			Group group = new Group();
			PopulateDropDownLists(group);
			return View(group);
		}

		// POST: Group/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,Description,GroupType,Status,ColourID")] Group @group)
		{
			if (ModelState.IsValid)
			{
				_context.Add(@group);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(@group);
		}

		// GET: Group/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups.FindAsync(id);
			if (@group == null)
			{
				return NotFound();
			}
			PopulateDropDownLists(@group);
			return View(@group);
		}

		// POST: Group/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id)
		{
			var groupToUpdate = await _context.Groups
				.FirstOrDefaultAsync(g => g.ID == id);

			if (groupToUpdate == null)
				return NotFound();

			try
			{
				if (await TryUpdateModelAsync(groupToUpdate,
					"",
					g => g.Name,
					g => g.Description,
					g => g.GroupType,
					g => g.Status,
					g => g.ColourID))
				{
					try
					{
						await _context.SaveChangesAsync();
						AddSuccessToast(groupToUpdate.Name);
						return RedirectToAction("Details", new { groupToUpdate.ID });
					}
					catch (RetryLimitExceededException)
					{
						ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please Try Again.");
					}
					catch (DbUpdateConcurrencyException)
					{
						if (!GroupExists(groupToUpdate.ID))
						{
							return NotFound();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetBaseException().ToString());
			}
			PopulateDropDownLists(groupToUpdate);
			return View(groupToUpdate);
		}

		// GET: Group/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups
				.FirstOrDefaultAsync(m => m.ID == id);
			if (@group == null)
			{
				return NotFound();
			}

			return View(@group);
		}

		// POST: Group/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var @group = await _context.Groups.FindAsync(id);
			if (@group != null)
			{
				_context.Groups.Remove(@group);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		// POST: Group/Email/
		[HttpPost, ActionName("Email")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Email(string selectedItems)
		{
			Debug.WriteLine(selectedItems);
			return RedirectToAction(nameof(Index));
		}

		// POST: Group/Export/
		[HttpPost, ActionName("Export")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Export(string selectedItems)
		{
			Debug.WriteLine(selectedItems);
			return RedirectToAction(nameof(Index));
		}

		private void PopulateDropDownLists(Group? group = null)
		{
			ViewData["BackgroundColour"] = ColourSelectList(group?.ColourID);
		}

		private bool GroupExists(int id)
		{
			return _context.Groups.Any(e => e.ID == id);
		}
	}
}