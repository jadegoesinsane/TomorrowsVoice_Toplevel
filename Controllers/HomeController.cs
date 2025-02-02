using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Diagnostics;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.ViewModels;

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
			var model = new HomeVM
			{
				SingerCount = _context.Singers.Where(s => s.Status == Status.Active).Count(),
				RehearsalCount = _context.Rehearsals.Where(r => r.Status == Status.Active).Count(),
				DirectorCount = _context.Directors.Where(s => s.Status == Status.Active).Count(),
				ChapterCount = _context.Chapters.Where(c => c.Status == Status.Active).Count()
			};
			return View(model);
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