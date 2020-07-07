using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class PostController : Controller
	{
		private readonly IPostService _postService;
		private readonly IImageService _imageService;

		public PostController(IPostService postService, IImageService imageService)
		{
			_postService = postService;
			_imageService = imageService;
		}

		[HttpGet("/blog/{urlTitle}")]
		public async Task<IActionResult> Detail(string urlTitle)
		{
			var post = await _postService.Get(urlTitle);
			if (post == null)
				return StatusCode(404);
			return View(post);
		}

		[HttpGet("/post/image/{id}")]
		public async Task<ActionResult> Image(int id)
		{
			var image = await _imageService.GetImage(id);
			if (image == null)
				return StatusCode(404);
			if (!String.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
			{
				CultureInfo provider = CultureInfo.InvariantCulture;
				var lastMod = DateTime.ParseExact(Request.Headers["If-Modified-Since"], "r", provider);
				if (lastMod == image.TimeStamp.AddMilliseconds(-image.TimeStamp.Millisecond))
				{
					Response.StatusCode = 304;
					return Content(String.Empty);
				}
			}
			var stream = new MemoryStream(await _imageService.GetImageData(image.ImageID));
			Response.Headers.Add("Cache-Control", "public");
			Response.Headers.Add("Last-Modified", image.TimeStamp.ToString("r"));
			return File(stream, image.MimeType);
		}
	}
}