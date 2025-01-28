using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
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
		//This is the list of Actions that will add the ReturnURL to ViewData
		internal string[] ActionWithURL = [ "Details", "Create", "Edit", "Delete",
			"Add", "Update", "Remove" ];

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

		// Alert Controller
		public void Success(string message, bool dismissable = false)
		{
			AddAlert(AlertStyles.Success, message, dismissable);
		}

		public void Information(string message, bool dismissable = false)
		{
			AddAlert(AlertStyles.Information, message, dismissable);
		}

		public void Warning(string message, bool dismissable = false)
		{
			AddAlert(AlertStyles.Warning, message, dismissable);
		}

		public void Danger(string message, bool dismissable = false)
		{
			AddAlert(AlertStyles.Danger, message, dismissable);
		}

		private void AddAlert(string alertStyle, string message, bool dismissable)
		{
			List<Alert> alerts = new List<Alert>();

			if (TempData.ContainsKey(Alert.TempDataKey) && TempData[Alert.TempDataKey] != null)
			{
				var tempDataValue = TempData[Alert.TempDataKey].ToString();
				if (!string.IsNullOrEmpty(tempDataValue))
				{
					alerts = JsonConvert.DeserializeObject<List<Alert>>(tempDataValue);
				}
			}

			alerts.Add(new Alert
			{
				AlertStyle = alertStyle,
				Message = message,
				Dismissable = dismissable
			});

			TempData[Alert.TempDataKey] = JsonConvert.SerializeObject(alerts);
		}
	}
}