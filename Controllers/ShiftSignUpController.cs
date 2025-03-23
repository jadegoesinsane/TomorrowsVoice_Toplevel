using System;
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
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin, Planner, Volunteer")]
	public class ShiftSignUpController : ElephantController
    {
        private readonly TVContext _context;

        public ShiftSignUpController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
        {
            _context = context;
        }

        [Authorize(Roles ="Admin. Planner, Volunteer")]
		// GET: ShiftSignUp
		public async Task<IActionResult> Index( int? EventID, int? CityID, string? actionButton, int? page, int? pageSizeID,
            DateTime StartDate, DateTime EndDate, string sortDirection = "asc", string sortField = "Date" )
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

		[Authorize(Roles = "Admin. Planner, Volunteer")]
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
