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
            string? SearchString, 
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
                int dayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dayInMonth);
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
                .Where(a => a.StartAt >= StartDate && a.StartAt <= EndDate)
                .AsNoTracking();
                //.GroupBy(s => s.ShiftDate)
                //.Select(g => g.OrderBy(s => s.ID).First())
                

            // Filters
            if (CityID.HasValue)
            {
                //shifts = shifts.Where(s=>s.Event.CityEvents == CityID);
                numFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                shifts = shifts.Where(r => r.Event.Name.ToUpper().Contains(SearchString.ToUpper()));
                numFilters++;
            }

            ViewData["AptCount"] = shifts.Count();

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

        public async Task <IActionResult> DateShift(DateTime date, int? page, int? pageSizeID)
        {
            //var shift = _context.Shifts.Where(s => s.ID == id).FirstOrDefault();
            //var date = shift.ShiftDate;

            var shifts = _context.Shifts
                .Include(s => s.Event)
                .Where(s=>s.ShiftDate == date && s.Status == Status.Active);

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

        [HttpPost]
        [Route("ShiftSignUp/Test/{shiftID}/{volID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VolunteerShifts(int shiftID, int volID)
        {
            var userShift = new UserShift
            {
                UserID = volID,
                ShiftID = shiftID
            };

            var shift = _context.Shifts.Where(s=>s.ID == shiftID).FirstOrDefaultAsync();

            try
            {
                _context.Add(userShift);
                await _context.SaveChangesAsync();
                //AddSuccessToast("Sucessfully signed up for shift on " + shift.StartAt.ToLongDateString());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
            }
            var shifts = _context.UserShifts
                .Include(s => s.Shift)
                .ThenInclude(s=>s.Event)
                .Where(s => s.UserID == volID);

            string name = _context.Volunteers.Where(v => v.ID == volID).Select(v => v.NameFormatted).FirstOrDefault();
            ViewData["VolunteerName"] = name;

            return View(shifts);
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
                .OrderBy(v=>v.LastName), "ID", "NameFormatted");
        }

        public void populateLists()
        {
            ViewData["VolunteerID"] = VolunteerSelectList();
            ViewData["CityID"] = CitySelectList();
        }

        //// GET: ShiftSignUp/Create
        //public IActionResult Create()
        //{
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID");
        //    return View();
        //}

        //// POST: ShiftSignUp/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(shift);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// GET: ShiftSignUp/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var shift = await _context.Shifts.FindAsync(id);
        //    if (shift == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// POST: ShiftSignUp/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,StartAt,EndAt,VolunteersNeeded,EventID")] Shift shift)
        //{
        //    if (id != shift.ID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(shift);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ShiftExists(shift.ID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EventID"] = new SelectList(_context.Events, "ID", "ID", shift.EventID);
        //    return View(shift);
        //}

        //// GET: ShiftSignUp/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var shift = await _context.Shifts
        //        .Include(s => s.Event)
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (shift == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(shift);
        //}

        //// POST: ShiftSignUp/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var shift = await _context.Shifts.FindAsync(id);
        //    if (shift != null)
        //    {
        //        _context.Shifts.Remove(shift);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
