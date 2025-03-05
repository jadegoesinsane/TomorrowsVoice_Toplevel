﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Messaging;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class VolunteerforUserController : ElephantController
    {
        private readonly TVContext _context;

        public VolunteerforUserController(TVContext context, IMyEmailSender emailSender, IToastNotification toastNotification) : base(context, toastNotification)
        {
            _context = context;

            
        }

        // GET: VolunteerforUser
        public async Task<IActionResult> Index(string? SearchString, int? page, int? pageSizeID, string? actionButton, string? StatusFilter, string sortField = "Volunteer", string sortDirection = "asc")
        {
            string[] sortOptions = new[] { "Volunteer", "Hours Volunteered", "Participation", "Absences" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            Enum.TryParse(StatusFilter, out Status selectedDOW);


            var statusList = Enum.GetValues(typeof(Status))
                         .Cast<Status>()
                         .Where(s => s == Status.Active || s == Status.Archived)
                         .ToList();


            ViewBag.StatusList = new SelectList(statusList);
            var volunteers = _context.Volunteers.AsNoTracking();
            if (!String.IsNullOrEmpty(StatusFilter))
            {
                volunteers = volunteers.Where(p => p.Status == selectedDOW);

                // filter out archived singers if the user does not specifically select "archived"
                if (selectedDOW != Status.Archived)
                {
                    volunteers = volunteers.Where(s => s.Status != Status.Archived);
                }
                numberFilters++;
            }
            else
            {
                volunteers = volunteers.Where(s => s.Status != Status.Archived);
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                volunteers = volunteers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            if (sortField == "Volunteer")
            {
                ViewData["hourVolSort"] = "fa fa-solid fa-sort";
                ViewData["partiSort"] = "fa fa-solid fa-sort";
                ViewData["absenceSort"] = "fa fa-solid fa-sort";
                if (sortDirection == "asc")
                {
                    volunteers = volunteers
                        .OrderBy(v => v.LastName)
                        .ThenBy(v => v.FirstName);
                    ViewData["volunteerSort"] = "fa fa-solid fa-sort-up";
                }
                else
                {
                    volunteers = volunteers
                        .OrderByDescending(v => v.LastName)
                        .ThenBy(v => v.FirstName);
                    ViewData["volunteerSort"] = "fa fa-solid fa-sort-down";
                }
            }
            else if (sortField == "Hours Volunteered")
            {
                ViewData["volunteerSort"] = "fa fa-solid fa-sort";
                ViewData["partiSort"] = "fa fa-solid fa-sort";
                ViewData["absenceSort"] = "fa fa-solid fa-sort";
                if (sortDirection == "asc")
                {
                    volunteers = volunteers.OrderBy(v => v.HoursVolunteered);
                    ViewData["hourVolSort"] = "fa fa-solid fa-sort-up";
                }
                else
                {
                    volunteers = volunteers.OrderByDescending(v => v.HoursVolunteered);
                    ViewData["hourVolSort"] = "fa fa-solid fa-sort-down";
                }
            }
            else if (sortField == "Participation")
            {
                ViewData["volunteerSort"] = "fa fa-solid fa-sort";
                ViewData["hourVolSort"] = "fa fa-solid fa-sort";
                ViewData["absenceSort"] = "fa fa-solid fa-sort";
                if (sortDirection == "asc")
                {
                    volunteers = volunteers.OrderBy(v => v.ParticipationCount);
                    ViewData["partiSort"] = "fa fa-solid fa-sort-up";
                }
                else
                {
                    volunteers = volunteers.OrderByDescending(v => v.ParticipationCount);
                    ViewData["partiSort"] = "fa fa-solid fa-sort-down";
                }
            }
            else if (sortField == "Absences")
            {
                ViewData["volunteersSort"] = "fa fa-solid fa-sort";
                ViewData["hourVolSort"] = "fa fa-solid fa-sort";
                if (sortDirection == "asc")
                {
                    volunteers = volunteers.OrderBy(v => v.absences);
                    ViewData["absenceSort"] = "fa fa-solid fa-sort-up";
                }
                else
                {
                    volunteers = volunteers.OrderByDescending(v => v.absences);
                    ViewData["absenceSort"] = "fa fa-solid fa-sort-down";
                }
            }


            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // GET: VolunteerforUser/Details/5
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

        // GET: VolunteerforUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VolunteerforUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoursVolunteered,totalWorkDuration,ParticipationCount,absences,YearlyVolunteerGoal,ID,Nickname,FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(volunteer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(volunteer);
        }

        // GET: VolunteerforUser/Edit/5
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

        // POST: VolunteerforUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HoursVolunteered,totalWorkDuration,ParticipationCount,absences,YearlyVolunteerGoal,ID,Nickname,FirstName,MiddleName,LastName,Email,Phone,Status")] Volunteer volunteer)
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

        // GET: VolunteerforUser/Delete/5
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

        // POST: VolunteerforUser/Delete/5
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


        public async Task<IActionResult> ShiftDetails(int? shiftId,int? volunteerID)
        {

            ViewBag.volunteerID = volunteerID;
            if (shiftId == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .Include(s => s.UserShifts)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(m => m.ID == shiftId);
            if (shift == null)
            {
                return NotFound();
            }
            return View(shift);
        }
        public PartialViewResult ListOfShiftDetails(int id)

        {
            var volunteer = _context.Volunteers
                            .FirstOrDefault(v => v.ID == id);
            ViewData["Volunteer"] = volunteer;
            var shifts = _context.Shifts
                  .Include(a => a.UserShifts).Include(c => c.Event)
                  .Where(vs => vs.Status != Status.Archived)
                  .Where(r => r.UserShifts.Any(ra => ra.UserID == id))
                  .OrderBy(r => r.ShiftDate)
                  .ToList();

            return PartialView("_ListOfShiftDetails", shifts);
        }
        public async Task<IActionResult> ShiftIndex(int id)
        {


            var volunteer = _context.Volunteers
                            .FirstOrDefault(v => v.ID == id);

            if (volunteer == null)
            {
                return NotFound();
            }
            ViewData["Volunteer"] = volunteer;

            var volunteerShifts = _context.UserShifts.Include(a => a.Shift)
                                   .Where(vs => vs.UserID == volunteer.ID).Where(vs => vs.Shift.Status != Status.Archived)
                                   .Select(vs => vs.ShiftID)
                                   .ToList();

            ViewData["VolunteerShifts"] = volunteerShifts;

            var tVContext = _context.Shifts.Include(s => s.Event);
            return View(await tVContext.ToListAsync());


        }

		public PartialViewResult GetMessages(int id)
		{
			ViewBag.ShiftID = id;
			var messages = _context.Messages
				.Where(m => m.ChatID == id)
				.Include(m => m.User)
				.Select(m => new MessageVM
				{
					Name = !string.IsNullOrEmpty(m.User.Nickname) ? m.User.Nickname : m.User.NameFormatted,
					CreatedOn = m.CreatedOn,
					Content = m.Content ?? string.Empty
				})
				.ToList();

			return PartialView("_GetMessages", messages);
			
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


		private bool VolunteerExists(int id)
        {
            return _context.Volunteers.Any(e => e.ID == id);
        }
    }
}
