﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.CustomControllers;
using NToastNotify;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class ShiftSignUpController : ElephantController
    {
        private readonly TVContext _context;

        public ShiftSignUpController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
        {
            _context = context;
        }

        // GET: ShiftSignUp
        public async Task<IActionResult> Index(
			int? EventID,
			int? CityID,
            string? actionButton,
            int? page,
            int? pageSizeID,
            DateTime StartDate,
            DateTime EndDate,
            string sortDirection = "asc",
            string sortField = "Date"
           )
        {
            if (EndDate == DateTime.MinValue)
            {
                //int dayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                EndDate = new DateTime(DateTime.Now.Year, 12, 31);
            }
            //Check the order of the dates and swap them if required
            if (EndDate < StartDate)
            {
                DateTime temp = EndDate;
                EndDate = StartDate;
                StartDate = temp;
            }
            //Save to View Data
            ViewData["StartDate"] = StartDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = EndDate.ToString("yyyy-MM-dd");

            // Sort Options
            string[] sortOpts = new[] { "Date" };

            // Counts number of filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            populateLists();

            var shifts = _context.Shifts
                .Include(s => s.Event)
                .ThenInclude(e => e.CityEvents)
                .ThenInclude(e => e.City)
                .Where(s => s.Status != Status.Archived && s.VolunteersNeeded > s.UserShifts.Count())
                .Where(a => a.StartAt >= StartDate && a.StartAt <= EndDate)
                .AsNoTracking();
            //.GroupBy(s => s.ShiftDate)
            //.Select(g => g.OrderBy(s => s.ID).First())


            // Filters
            if (CityID.HasValue)
            {
                shifts = shifts.Where(s => s.Event.CityEvents.Any(ce => ce.CityID == CityID));
                numFilters++;
            }
			if (EventID.HasValue)
			{
				shifts = shifts.Where(s => s.EventID == EventID);
				numFilters++;
			}

			ViewData["ShiftCount"] = shifts.Count();

            // Show how many filters are applied
            if (numFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = "(" + numFilters.ToString()
                    + " Filter" + (numFilters > 1 ? "s" : "") + " Applied)";
                @ViewData["ShowFilter"] = " show";
            }

            // Check for sorting change
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;
                if (sortOpts.Contains(actionButton))
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            // Sorting
            if (sortField == "Date")
            {
                if (sortDirection == "asc")
                {
                    shifts = shifts
                        .OrderByDescending(r => r.StartAt)
                        .ThenByDescending(r => r.StartAt);
                }
                else
                {
                    shifts = shifts
                        .OrderBy(r => r.StartAt)
                        .ThenBy(r => r.StartAt);
                }
            }

            // View data for next sort
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: ShiftSignUp/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }

            populateLists();

            return View(shift);
        }

        // GET: ShiftList Partial View
        public async Task<IActionResult> ShiftList(int? EventID, string? Location, DateTime FilterStartDate,
            int? page, int? pageSizeID, string? actionButton, string? StatusFilter)

        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

            if (!EventID.HasValue)
            {
                //Go back to the proper return url for the Events controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            Enum.TryParse(StatusFilter, out Status selectedStatus);

            populateLists();

            var shifts = _context.Shifts
                .Include(s => s.Event)
                .Include(s => s.UserShifts)
                .Where(s => s.EventID == EventID.GetValueOrDefault())
                .AsNoTracking();

            if (!String.IsNullOrEmpty(StatusFilter))
            {
                shifts = shifts.Where(p => p.Status == selectedStatus);

                // filter out archived events if the user does not specifically select "archived"
                if (selectedStatus != Status.Archived)
                {
                    shifts = shifts.Where(s => s.Status != Status.Archived);
                }
                numberFilters++;
            }
            // filter out events even if status filter has not been set
            else
            {
                shifts = shifts.Where(s => s.Status != Status.Archived);
            }
            //Filter For Start and End times
            if (FilterStartDate != default(DateTime))
            {
                if (FilterStartDate != default(DateTime))
                {
                    ViewData["StartDate"] = FilterStartDate.ToString("yyyy-MM-dd");
                }
                shifts = shifts.Where(e => e.ShiftDate == FilterStartDate);
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
            }

            // MASTER Record, Events
            Event? events = await _context.Events.FirstOrDefaultAsync(e => e.ID == EventID.GetValueOrDefault());

            ViewBag.Event = events;


            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

            return PartialView("_ShiftListSignUp", pagedData);
        }
            
        public async Task<IActionResult> DateShift(DateTime date, int? page, int? pageSizeID)
        {
            //var shift = _context.Shifts.Where(s => s.ID == id).FirstOrDefault();
            //var date = shift.ShiftDate;

            var shifts = _context.Shifts
                .Include(s => s.Event).Include(s => s.UserShifts)
                .Where(s => s.ShiftDate == date && s.Status == Status.Active && s.VolunteersNeeded > s.UserShifts.Count());

            ViewData["Date"] = date.ToLongDateString();

            // Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        [HttpPost]
        [Route("ShiftSignUp/Details/{shiftID}/{volID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int shiftID, int volID)
        {
            var userShift = new UserShift
            {
                UserID = volID,
                ShiftID = shiftID
            };

            try
            {
                _context.Add(userShift);
                await _context.SaveChangesAsync();
                AddSuccessToast("Sucessfully signed up for shift on " + userShift.Shift.StartAt.ToLongDateString());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
            }
            var shift = _context.Shifts.Where(s => s.ID == shiftID);

            return View(shift);
        }

        //[HttpPost]
        //[Route("ShiftSignUp/Test/{shiftID}/{volID}")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VolunteerShifts(int shiftID, int volID)
        //{
        //    // create new volunteer signup
        //    var userShift = new UserShift
        //    {
        //        UserID = volID,
        //        ShiftID = shiftID
        //    };

        //    // get date for success toast
        //    string date = _context.Shifts.Where(s => s.ID == shiftID).Select(s => s.StartAt.ToLongDateString()).FirstOrDefault();
        //    // get name for success toast
        //    string name = _context.Volunteers.Where(v => v.ID == volID).Select(v => v.NameFormatted).FirstOrDefault();
        //    // get event for success toast
        //    int eventID = _context.Shifts.Where(s => s.ID == shiftID).Select(s => s.EventID).FirstOrDefault();
        //    string event_ = _context.Events.Where(e => e.ID == eventID).Select(e => e.Name).FirstOrDefault();


        //    try
        //    {
        //        _context.Add(userShift);
        //        await _context.SaveChangesAsync();
        //        AddSignUpToast(date, name, event_);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
        //    }

        //    return RedirectToAction($"Details", "Volunteer", new { id = volID });
        //}


        private SelectList CitySelectList()
        {
            return new SelectList(_context
                .Cities
                .OrderBy(c => c.Name), "ID", "Name");
        }

        private SelectList VolunteerSelectList()
        {
            return new SelectList(_context
                .Volunteers
                .OrderBy(v => v.LastName), "ID", "NameFormatted");
        }

		private SelectList EventSelectList()
		{
			return new SelectList(_context
				.Events
				.OrderBy(c => c.Name), "ID", "Name");
		}

		public void populateLists()
        {
            ViewData["VolunteerID"] = VolunteerSelectList();
            ViewData["CityID"] = CitySelectList();
            ViewData["EventID"] = EventSelectList();

            var statusList = Enum.GetValues(typeof(Status))
                         .Cast<Status>()
                         .Where(s => s == Status.Active || s == Status.Canceled || s == Status.Archived)
                         .ToList();

            ViewBag.StatusList = new SelectList(statusList);
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
