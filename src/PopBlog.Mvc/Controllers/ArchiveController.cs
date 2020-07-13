using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class ArchiveController : Controller
	{
		private readonly IPostService _postService;

		public ArchiveController(IPostService postService)
		{
			_postService = postService;
		}

		[HttpGet("/Archive")]
		[ResponseCache(Duration = 600)]
		public async Task<IActionResult> Index()
		{
			var archiveCounts = await _postService.GetArchiveCounts();
			return View(archiveCounts);
		}

		[HttpGet("/Archive/{year}/{month}")]
		public async Task<IActionResult> Month(int year, int month)
		{
			var posts = await _postService.GetPostsByMonth(year, month);
			ViewBag.Year = year;
			ViewBag.Month = month;
			return View(posts);
		}
	}
}