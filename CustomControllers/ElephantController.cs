using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NToastNotify;
using System.IO;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.CustomControllers
{
	/// <summary>
	/// The Elephant Controller has a good memory to help
	/// persist the Index Sort, Filter and Paging parameters
	/// into a URL stored in ViewData
	/// WARNING: Depends on the following Utilities
	///  - CookieHelper
	///  - MaintainURL
	/// </summary>
	public class ElephantController : CognizantController
	{
		private readonly TVContext _context;
		protected readonly IToastNotification _toastNotification;

		//This is the list of Actions that will add the ReturnURL to ViewData
		internal string[] ActionWithURL = [ "Details", "Create", "Edit", "Delete",
			"Add", "Update", "Remove" ];

		public ElephantController(TVContext context, IToastNotification toastNotification)
		{
			_context = context;
			_toastNotification = toastNotification;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (ActionWithURL.Contains(ActionName()))
			{
				ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
			}
			else if (ActionName() == "Index")
			{
				//Clear the sort/filter/paging URL Cookie for Controller
				CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
			}
			base.OnActionExecuting(context);
		}

		public override Task OnActionExecutionAsync(
			ActionExecutingContext context,
			ActionExecutionDelegate next)
		{
			if (ActionWithURL.Contains(ActionName()))
			{
				ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
			}
			else if (ActionName() == "Index")
			{
				//Clear the sort/filter/paging URL Cookie for Controller
				CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
			}
			return base.OnActionExecutionAsync(context, next);
		}

		#region Select Lists

		// Get Cities (With chapters of a certain status?)
		internal SelectList CitySelectList(int? id, Status? status = null)
		{
			IQueryable cities;
			if (status.HasValue)
			{
				var chapters = _context.Chapters.Where(c => c.Status == status);
				cities = _context.Cities
					.Join(chapters,
						  city => city.ID,
						  chapter => chapter.CityID,
						  (city, chapter) => city)
					.Distinct()
					.OrderBy(c => c.Name);

				return new SelectList(cities, "ID", "Name", id);
			}
			cities = _context
				.Cities
				.OrderBy(c => c.Name);
			return new SelectList(cities, "ID", "Name", id);
		}

        // Get Directors
        internal SelectList EventSelectList(int? id, Status? status = null)
        {
            var events = _context.Events
                    .OrderBy(s => s.Name);

            if (status.HasValue)
                events = events.Where(d => d.Status == status)
                    .OrderBy(s => s.Name);

            return new SelectList(events, "ID", "Name", id);
        }

        internal SelectList DirectorSelectList(int? id, Status? status = null)
		{
			var directors = _context.Directors
					.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

			if (status.HasValue)
				directors = directors.Where(d => d.Status == status)
					.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

			return new SelectList(directors, "ID", "NameFormatted", id);
		}

		// Get Singers

		internal SelectList SingerSelectList(int? id, Status? status = null)
		{
			var singers = _context.Singers
					.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

			if (status.HasValue)
				singers = singers.Where(d => d.Status == status)
					.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

			return new SelectList(singers, "ID", "NameFormatted", id);
		}

		// Get Chapters

		//internal SelectList ChapterSelectList(int? id, Status? status = null)
		//{
		//	var chapters = _context.Chapters
		//		.OrderBy(c => c.City.Name);

		//	if (status.HasValue)
		//		chapters = chapters
		//			.Where(c => c.Status == status)
		//			.OrderBy(c => c.City.Name);

		//	return new SelectList(chapters, "ID", "Name", id);
		//}

		// Get Event 
		internal IEnumerable<string> EventLocationSelectList()
		{
			var locations = _context.Events
				.OrderBy(e => e.Location)
				.AsNoTracking()
				.Select(e => e.Location)
				.Distinct();

			return locations.ToList();
		}

		#endregion Select Lists

		// Toasty Notifications
		// Success
		protected void AddSuccessToast(string name)
		{
			string action = ViewData["ActionName"]?.ToString().ToLower();
			if (action.EndsWith('e'))
				action += "d.";
			else
				action += "ed.";
			string message = $"{name} was successfully {action}";
			_toastNotification.AddSuccessToastMessage(message);
		}

		// Custom Notification for Signing up for shift
		protected void AddSignUpToast(string date, string name, string event_)
		{
			string message = $"{name} successfully signed up for {event_} shift on {date}";
			_toastNotification.AddSuccessToastMessage(message);
		}

		// Error
		protected void AddErrorToast(string message)
		{
			_toastNotification.AddErrorToastMessage(message);
		}
	}
}