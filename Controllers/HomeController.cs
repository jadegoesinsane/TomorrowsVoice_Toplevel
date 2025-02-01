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

		public string SingerCount()
		{
            return $"{_context.Singers.Where(s => s.Status == Status.Active).Count()} Singers";
		}
		public string RehearsalCount()
		{
			return $"{_context.Rehearsals.Where(r => r.Status == Status.Active).Count()} Rehearsals";
		}
        public string DirectorCount()
        {
			return $"{_context.Directors.Where(s => s.Status == Status.Active).Count()} Directors";
		}
        public string ChapterCount()
        {
			return $"{_context.Chapters.Where(c => c.Status == Status.Active).Count()} Chapters";
		}
	}
}
