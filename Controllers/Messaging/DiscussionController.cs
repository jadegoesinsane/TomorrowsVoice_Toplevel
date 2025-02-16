using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Messaging;

namespace TomorrowsVoice_Toplevel.Controllers.Messaging
{
	public class DiscussionController : Controller
	{
		private readonly TVContext _context;

		public DiscussionController(TVContext context)
		{
			_context = context;
		}

		// GET: Discussion
		public async Task<IActionResult> Index()
		{
			var tVContext = _context.Discussions.Include(d => d.Shift);
			return View(await tVContext.ToListAsync());
		}

		// GET: Discussion/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var discussion = await _context.Discussions
				.Include(d => d.Shift)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (discussion == null)
			{
				return NotFound();
			}

			return View(discussion);
		}

		// GET: Discussion/Create
		public IActionResult Create()
		{
			ViewData["ShiftID"] = new SelectList(_context.Shifts, "ID", "ID");
			return View();
		}

		// POST: Discussion/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,ShiftID,Title")] Discussion discussion)
		{
			if (ModelState.IsValid)
			{
				_context.Add(discussion);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ShiftID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ShiftID);
			return View(discussion);
		}

		// GET: Discussion/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var discussion = await _context.Discussions.FindAsync(id);
			if (discussion == null)
			{
				return NotFound();
			}
			ViewData["ShiftID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ShiftID);
			return View(discussion);
		}

		// POST: Discussion/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,ShiftID,Title")] Discussion discussion)
		{
			if (id != discussion.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(discussion);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DiscussionExists(discussion.ID))
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
			ViewData["ShiftID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ShiftID);
			return View(discussion);
		}

		// GET: Discussion/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var discussion = await _context.Discussions
				.Include(d => d.Shift)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (discussion == null)
			{
				return NotFound();
			}

			return View(discussion);
		}

		// POST: Discussion/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var discussion = await _context.Discussions.FindAsync(id);
			if (discussion != null)
			{
				_context.Discussions.Remove(discussion);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public PartialViewResult ListOfMessages(int id)
		{
			var messages = _context.Messages
				.Include(r => r.Volunteer)
				.Where(r => r.DiscussionID == id)
				.ToList();
			return PartialView("_ListOfMessages", messages);
		}

		private bool DiscussionExists(int id)
		{
			return _context.Discussions.Any(e => e.ID == id);
		}
	}
}