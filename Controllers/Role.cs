using Microsoft.AspNetCore.Mvc;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class Role : Controller
    {
        public IActionResult Index(string role)
        {
            if (string.IsNullOrEmpty(role))
            {

                return View();
            }

            if (role == "Singer")
            {
                return RedirectToAction("Create", "Singer");
            }
            else if (role == "Director")
            {
                return RedirectToAction("Create", "Director");
            }

            return View();
        }

    }
}
