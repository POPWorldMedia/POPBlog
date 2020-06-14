using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private readonly IUserService _userService;

		public HomeController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet("/")]
		public async Task<IActionResult> Index()
		{
			return View();
		}

		[HttpGet("/setup")]
		public async Task<IActionResult> Setup()
		{
			if (await _userService.IsFirstUserCreated())
				return StatusCode(403);
			return View();
		}

		[HttpPost("/setup")]
		public async Task<IActionResult> Setup(User user)
		{
			if (await _userService.IsFirstUserCreated())
				return StatusCode(403);
			await _userService.Create(user);
			return RedirectToAction("Index");
		}
	}
}
