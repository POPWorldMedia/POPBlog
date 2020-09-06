using System;
using System.Globalization;
using System.IO;
using System.Security.Claims;
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
		private readonly IReCaptchaService _reCaptchaService;
		private readonly IIpBanService _ipBanService;

		public PostController(IPostService postService, IImageService imageService, ICommentService commentService, IReCaptchaService reCaptchaService, IIpBanService ipBanService)
		{
			_postService = postService;
			_imageService = imageService;
			_commentService = commentService;
			_reCaptchaService = reCaptchaService;
			_ipBanService = ipBanService;
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
		public async Task<IActionResult> Image(int id)
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
		public async Task<IActionResult> AddComment(CommentPost comment)
		{
			var post = await _postService.Get(comment.PostID);
			if (post == null)
				return StatusCode(404);
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			if (!User.HasClaim(ClaimTypes.Role, "Admin"))
			{
				var recap = await _reCaptchaService.VerifyToken(comment.Token, ip);
				if (!recap.IsSuccess)
					return StatusCode(403);
				var isIpBanned = await _ipBanService.IsIpBanned(ip);
				if (isIpBanned)
					return StatusCode(403);
			}
			var commentID = await _commentService.Create(comment.PostID, comment.FullText, comment.Name, comment.Email, comment.WebSite);
			return new RedirectResult(Url.Action("Detail", new { urlTitle = post.UrlTitle }) + "#" + commentID);
		}

		[HttpGet("/post/download/{id}")]
		[HttpHead("/post/download/{id}")]
		public async Task<IActionResult> Download(int id)
		{
			var link = await _postService.GetDownloadLink(id);
			if (string.IsNullOrEmpty(link))
				return StatusCode(404);
			await _postService.IncrementDownloadCount(id);
			return Redirect(link);
		}
	}
}