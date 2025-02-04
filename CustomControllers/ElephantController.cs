using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using NToastNotify;
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
		protected readonly IToastNotification _toastNotification;

		//This is the list of Actions that will add the ReturnURL to ViewData
		internal string[] ActionWithURL = [ "Details", "Create", "Edit", "Delete",
			"Add", "Update", "Remove" ];

		public ElephantController(IToastNotification toastNotification)
		{
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

		// Error
		protected void AddErrorToast(string message)
		{
			_toastNotification.AddErrorToastMessage(message);
		}
	}
}