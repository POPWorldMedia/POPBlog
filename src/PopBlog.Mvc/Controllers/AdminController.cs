using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	[Authorize(Policy = "Admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class AdminController : Controller
	{
		private readonly IPostService _postService;
		private readonly IUserService _userService;

		public AdminController(IPostService postService, IUserService userService)
		{
			_postService = postService;
			_userService = userService;
		}

		[HttpGet("/admin")]
		public async Task<IActionResult> Index()
		{
			return View();
		}

		[HttpGet("/admin/newpost")]
		public IActionResult NewPost()
		{
			var post = new Post {TimeStamp = DateTime.UtcNow};
			return View(post);
		}

		[HttpPost("/admin/newpost")]
		public async Task<IActionResult> NewPost(Post post)
		{
			var user = await _userService.GetUserByName(User.Identity.Name);
			post.UserID = user.UserID;
			await _postService.Create(post);
			return RedirectToAction("Index");
		}
	}
}