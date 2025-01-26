using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;

namespace TomorrowsVoice_Toplevel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly TVContext _context;

		public HomeController(ILogger<HomeController> logger, TVContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		public int SingerCount()
		{
			return _context.Singers.Count();
		}
		public int RehearsalCount()
		{
			return _context.Rehearsals.Count();
		}
        public int DirectorCount()
        {
            return _context.Directors.Count();
        }
        public int ChapterCount()
        {
            return _context.Directors.Count();
        }
	}
}
