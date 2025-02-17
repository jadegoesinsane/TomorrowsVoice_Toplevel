using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Messaging;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers.Messaging
{
	public class ChatController : Controller
	{
		private readonly TVContext _context;

		public ChatController(TVContext context)
		{
			_context = context;
		}
		public PartialViewResult GetMessages(int id)
		{
			var chat = _context.Chats.FirstOrDefault(c => c.ShiftID == id);
			var messages = _context.Messages
				.Where(m => m.ChatID == chat.ID)
				.OrderBy(m => m.CreatedOn)
				.Select(m => new MessageVM
				{
					Content = m.Content,
					CreatedOn = m.CreatedOn,
					VolunteerName = m.Volunteer.NameFormatted,
					VolunteerAvatar = m.Volunteer.Avatar
				})
				.ToList();
			return PartialView("_GetMessages", messages);
		}

		public IActionResult SendMessage(int shiftID, int volunteerID, string content)
		{
			var chat = _context.Chats.FirstOrDefault(c => c.ShiftID == shiftID);
			if (chat == null)
			{
				chat = new Chat { ShiftID = shiftID };
				_context.Chats.Add(chat);
				_context.SaveChanges();
			}
			Volunteer volunteer = _context.Volunteers.FirstOrDefault(v => v.ID == volunteerID);
			var message = new Message
			{
				ChatID = chat.ID,
				FromAccountID = volunteerID,
				Content = content,
				Volunteer = volunteer
			};

			_context.Messages.Add(message);
			_context.SaveChanges();

			return RedirectToAction("Details", new { id = shiftID });
		}

		// GET: Discussion
		public async Task<IActionResult> Index()
		{
			var tVContext = _context.Chats.Include(d => d.Shift);
			return View(await tVContext.ToListAsync());
		}

		// GET: Discussion/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var discussion = await _context.Chats
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
		public async Task<IActionResult> Create([Bind("ID,ShiftID,Title")] Chat discussion)
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

			var discussion = await _context.Chats.FindAsync(id);
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
		public async Task<IActionResult> Edit(int id, [Bind("ID,ShiftID,Title")] Chat discussion)
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

			var discussion = await _context.Chats
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
			var discussion = await _context.Chats.FindAsync(id);
			if (discussion != null)
			{
				_context.Chats.Remove(discussion);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool DiscussionExists(int id)
		{
			return _context.Chats.Any(e => e.ID == id);
		}
	}
}