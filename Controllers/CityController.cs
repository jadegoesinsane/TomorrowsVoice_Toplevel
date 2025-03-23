using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin, Planner, Volunteer")]
	public class CityController : Controller
    {
        private readonly TVContext _context;

        public CityController(TVContext context)
        {
            _context = context;
        }

		[Authorize(Roles = "Admin, Planner, Volunteer")]
		// GET: City
		public async Task<IActionResult> Index()
        {
            return View(await _context.Cities.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        // GET: City/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .FirstOrDefaultAsync(m => m.ID == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }
        
        [Authorize(Roles = "Admin")]
        // GET: City/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: City/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name,Province")] City city)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(city);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { city.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                ExceptionMessageVM msg = new();
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                    msg.ErrMessage = "Cannot have duplicate cities in the same province.";
                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            //Decide if we need to send the Validaiton Errors directly to the client
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                //Was an AJAX request so build a message with all validation errors
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                //Note: returning a BadRequest results in HTTP Status code 400
                return BadRequest(errorMessage);
            }

            return View(city);
        }

        [Authorize(Roles = "Admin")]
        // GET: City/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cities == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return View(city);
        }

        // POST: City/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var cityToUpdate = await _context.Cities
                    .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (cityToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<City>(cityToUpdate, "",
                c => c.Name, c => c.Province))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { cityToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(cityToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ExceptionMessageVM msg = new();
                    ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
                }
            }
            return View(cityToUpdate);
        }

        [Authorize(Roles = "Admin")]
        // GET: City/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .FirstOrDefaultAsync(m => m.ID == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // POST: City/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities
                                         .Include(c => c.CityEvents)
                                         .Include(c => c.Chapters)
                                         .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
               

                if (city == null)
                {
                    return NotFound();
                }

              
                if (city.CityEvents.Any() || city.Chapters.Any())
                {
                 
                    throw new DbUpdateException("Cannot delete city because it has associated Chapters or CityEvents.");
                }

               
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {

                string message = dex.GetBaseException().Message;
                if (message.Contains("associated Chapters or CityEvents."))
                {
                    // Handling the custom exception
                    ModelState.AddModelError("", " Cannot delete city because it has associated Chapters or CityEvents.");
                }

            }

            return View(city);
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.ID == id);
        }

    }
}
