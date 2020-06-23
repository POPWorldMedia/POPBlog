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
			var last20 = await _postService.GetLast20();
			return View(last20);
		}

		[HttpGet("/admin/newpost")]
		public IActionResult NewPost()
		{
			var post = new Post {TimeStamp = DateTime.UtcNow, IsLive = true};
			return View(post);
		}

		[HttpPost("/admin/newpost")]
		public async Task<IActionResult> NewPost(Post post)
		{
			var user = await _userService.GetUserByName(User.Identity.Name);
			post.UserID = user.UserID;
			await _postService.Create(post);
			return RedirectToAction("Index", "Admin");
		}

		[HttpGet("/admin/editpost/{postID}")]
		public async Task<IActionResult> EditPost(int postID)
		{
			var post = await _postService.Get(postID);
			if (post == null)
				return StatusCode(404);
			return View(post);
		}

		[HttpPost("/admin/editpost/{postID}")]
		public async Task<IActionResult> EditPost(int postID, Post post)
		{
			post.PostID = postID;
			await _postService.Update(post);
			return RedirectToAction("Index", "Admin");
		}
	}
}