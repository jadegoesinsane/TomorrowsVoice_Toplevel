using System;
using System.Collections.Generic;
using System.IO;
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
			var chat = _context.Chats.FirstOrDefault(c => c.ID == id);
			var messages = _context.Messages
				.Where(m => m.ChatID == chat.ID)
				.OrderBy(m => m.CreatedOn)
				.ToList();
			//.Select(m => new MessageVM
			//{
			//	Content = m.Content,
			//	CreatedOn = m.CreatedOn,
			//	Name = m.User.NameFormatted,
			//	//Avatar = m.Volunteer.Avatar
			//})
			var messageVMs = new List<MessageVM>();
			foreach (var message in messages)
			{
				string name = "Unknown";

				var volunteer = _context.Volunteers.FirstOrDefault(v => v.ID == message.FromAccountID);
				if (volunteer != null)
				{
					name = !string.IsNullOrEmpty(volunteer.Nickname) ? volunteer.Nickname : volunteer.NameFormatted;
				}
				else
				{
					var director = _context.Directors.FirstOrDefault(d => d.ID == message.FromAccountID);
					if (director != null)
					{
						name = !string.IsNullOrEmpty(director.Nickname) ? director.Nickname : director.NameFormatted;
					}
				}

				messageVMs.Add(new MessageVM
				{
					Content = message.Content,
					CreatedOn = message.CreatedOn,
					Name = name,
					// Avatar = volunteer?.Avatar
				});
			}
			return PartialView("_GetMessages", messageVMs);
		}

		public IActionResult SendMessage(int shiftID, int volunteerID, string content)
		{
			var chat = _context.Chats.FirstOrDefault(c => c.ID == shiftID);
			if (chat == null)
			{
				chat = new Chat { ID = shiftID };
				_context.Chats.Add(chat);
				_context.SaveChanges();
			}
			Volunteer volunteer = _context.Volunteers.FirstOrDefault(v => v.ID == volunteerID);
			var message = new Message
			{
				ChatID = chat.ID,
				FromAccountID = volunteerID,
				Content = content,
				User = volunteer
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
			ViewData["ID"] = new SelectList(_context.Shifts, "ID", "ID");
			return View();
		}

		// POST: Discussion/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,ID,Title")] Chat discussion)
		{
			if (ModelState.IsValid)
			{
				_context.Add(discussion);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ID);
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
			ViewData["ID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ID);
			return View(discussion);
		}

		// POST: Discussion/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,ID,Title")] Chat discussion)
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
			ViewData["ID"] = new SelectList(_context.Shifts, "ID", "ID", discussion.ID);
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