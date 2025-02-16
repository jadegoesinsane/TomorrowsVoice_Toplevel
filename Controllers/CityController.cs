using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.CustomControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TomorrowsVoice_Toplevel.ViewModels;
using NToastNotify;

namespace TomorrowsVoice_Toplevel.Controllers
{
	//[Authorize(Roles = "Admin,Supervisor")]
	public class CityController : ElephantController
	{
		private readonly TVContext _context;

		public CityController(TVContext context, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_context = context;
		}

		// GET: City
		public IActionResult Index()
		{
			return Redirect(ViewData["returnURL"].ToString());
		}

		// GET: City/Create
		public IActionResult Create()
		{
			City city = new City();
			PopulateDropDownLists(city);
			return View(city);
		}

		// POST: City/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,Name,Province")] City city)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(city);
					await _context.SaveChangesAsync();
					return Redirect(ViewData["returnURL"].ToString());
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
					return Redirect(ViewData["returnURL"].ToString());
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

		// GET: Specialty/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Cities == null)
			{
				return NotFound();
			}

			var specialty = await _context.Cities
				.FirstOrDefaultAsync(m => m.ID == id);
			if (specialty == null)
			{
				return NotFound();
			}

			return View(specialty);
		}

		// POST: Specialty/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Cities == null)
			{
				return Problem("Entity set 'TVContext.Cities'  is null.");
			}
			var city = await _context.Cities
				.FirstOrDefaultAsync(m => m.ID == id);
			try
			{
				if (city != null)
				{
					_context.Cities.Remove(city);
				}
				await _context.SaveChangesAsync();
				return Redirect(ViewData["returnURL"].ToString());
			}
			catch (DbUpdateException dex)
			{
				ExceptionMessageVM msg = new();
				if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
				{
					msg.ErrProperty = "";
					msg.ErrMessage = "Unable to Delete " + ViewData["ControllerFriendlyName"] +
						". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] +
						" that has related records.";
				}
				ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
			}
			return View(city);
		}

		public void PopulateDropDownLists(City? city = null)
		{
			//ViewBag.Provinces =
		}

		//ViewBag.DirectorID = id;
		private bool CityExists(int id)
		{
			return _context.Cities.Any(e => e.ID == id);
		}
	}
}