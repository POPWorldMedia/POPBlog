using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class PostController : Controller
	{
		private readonly IPostService _postService;

		public PostController(IPostService postService)
		{
			_postService = postService;
		}

		[HttpGet("/blog/{urlTitle}")]
		public async Task<IActionResult> Detail(string urlTitle)
		{
			var post = await _postService.Get(urlTitle);
			if (post == null)
				return StatusCode(404);
			return View(post);
		}
	}
}