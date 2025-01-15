using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace TomorrowsVoice_Toplevel.CustomControllers
{
    /// <summary>
    /// Makes the controller "self aware" knowing it's own name
    /// and what Action was called.
    /// </summary>
    public class CognizantController : Controller
    {
        internal string ControllerName()
        {
            return ControllerContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        }
        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// This method uses a regular expression to split the input string at each 
        /// capital letter and then adds a space before each capital letter. 
        /// The Trim() method is used to remove any leading or trailing spaces.
        /// </summary>
        /// <param name="input">String in camel case to split into words</param>
        /// <returns>The camel case String split into words</returns>
        internal static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "([A-Z])", " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        /// <summary>
        /// This code is executed before any Action method is called
        /// and gives us a chance to add to the ViewData dictionary.
        /// Great way to make these values available to all Views.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Add the Controller and Action names to ViewData
            string ControllerFriendlyName = SplitCamelCase(ControllerName());
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = ControllerFriendlyName;
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerFriendlyName + " " + ActionName();
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Same as above but for async Actions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            //Add the Controller and Action names to ViewData
            string ControllerFriendlyName = SplitCamelCase(ControllerName());
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = ControllerFriendlyName;
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerFriendlyName + " " + ActionName();
            return base.OnActionExecutionAsync(context, next);
        }

    }
}
