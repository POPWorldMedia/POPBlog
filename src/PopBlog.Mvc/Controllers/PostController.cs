using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class PostController : Controller
	{
		private readonly IPostService _postService;
		private readonly IImageService _imageService;
		private readonly ICommentService _commentService;

		public PostController(IPostService postService, IImageService imageService, ICommentService commentService)
		{
			_postService = postService;
			_imageService = imageService;
			_commentService = commentService;
		}

		[HttpGet("/blog/{urlTitle}")]
		public async Task<IActionResult> Detail(string urlTitle)
		{
			var post = await _postService.Get(urlTitle);
			if (post == null)
				return StatusCode(404);
			var comments = await _commentService.GetByPost(post.PostID);
			var container = new PostDisplayContainer {Post = post, Comments = comments};
			return View(container);
		}

		[HttpGet("/post/image/{id}")]
		public async Task<ActionResult> Image(int id)
		{
			var image = await _imageService.GetImage(id);
			if (image == null)
				return StatusCode(404);
			if (!string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
			{
				var provider = CultureInfo.InvariantCulture;
				var lastMod = DateTime.ParseExact(Request.Headers["If-Modified-Since"], "r", provider);
				if (lastMod == image.TimeStamp.AddMilliseconds(-image.TimeStamp.Millisecond))
				{
					Response.StatusCode = 304;
					return Content(string.Empty);
				}
			}
			var stream = new MemoryStream(await _imageService.GetImageData(image.ImageID));
			Response.Headers.Add("Cache-Control", "public");
			Response.Headers.Add("Last-Modified", image.TimeStamp.ToString("r"));
			return File(stream, image.MimeType);
		}

		[HttpPost("/post/addcomment")]
		public async Task<ActionResult> AddComment(CommentPost comment)
		{
			var post = await _postService.Get(comment.PostID);
			if (post == null)
				return StatusCode(404);
			var commentID = await _commentService.Create(comment.PostID, comment.FullText, comment.Name, comment.Email, comment.WebSite);
			return new RedirectResult(Url.Action("Detail", new { urlTitle = post.UrlTitle }) + "#" + commentID);
		}
	}
}