using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class PostController : Controller
	{
		private readonly IPostService _postService;
		private readonly IImageService _imageService;
		private readonly ICommentService _commentService;
		private readonly ITurnstileService _turnstileService;
		private readonly IIpBanService _ipBanService;

		public PostController(IPostService postService, IImageService imageService, ICommentService commentService, ITurnstileService turnstileService, IIpBanService ipBanService)
		{
			_postService = postService;
			_imageService = imageService;
			_commentService = commentService;
			_turnstileService = turnstileService;
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
			// Images are immutable once uploaded, so they can be cached by browsers and shared caches (CDNs) indefinitely.
			// HTTP dates have one-second resolution, so truncate the stored (UTC) timestamp to match what we send.
			var utc = DateTime.SpecifyKind(image.TimeStamp, DateTimeKind.Utc);
			var lastModified = new DateTimeOffset(utc.AddTicks(-(utc.Ticks % TimeSpan.TicksPerSecond)));
			var etag = new EntityTagHeaderValue($"\"img-{image.ImageID}-{lastModified.ToUnixTimeSeconds()}\"");

			var responseHeaders = Response.GetTypedHeaders();
			responseHeaders.CacheControl = new CacheControlHeaderValue
			{
				Public = true,
				MaxAge = TimeSpan.FromDays(365),
				Extensions = { new NameValueHeaderValue("immutable") }
			};
			responseHeaders.LastModified = lastModified;
			responseHeaders.ETag = etag;

			// Answer conditional requests before opening the blob stream from the database.
			var requestHeaders = Request.GetTypedHeaders();
			var ifNoneMatch = requestHeaders.IfNoneMatch;
			var notModified = ifNoneMatch is { Count: > 0 }
				? ifNoneMatch.Any(x => x.Equals(EntityTagHeaderValue.Any) || x.Compare(etag, useStrongComparison: false))
				: requestHeaders.IfModifiedSince is { } since && since >= lastModified;
			if (notModified)
				return StatusCode(StatusCodes.Status304NotModified);

			var responseStream = await _imageService.GetImageStream(image.ImageID);
			Response.RegisterForDispose(responseStream);
			return File(responseStream.Stream, image.MimeType);
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
				var recap = await _turnstileService.VerifyToken(comment.Token, ip);
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
			// HEAD only asks about the file, and a tiny range starting at 0 is a client probing for size and
			// range support before the real request, so neither is a download.
			if (HttpMethods.IsGet(Request.Method) && !IsProbeRange(Request.GetTypedHeaders().Range))
				await _postService.IncrementDownloadCount(id);
			return Redirect(link);
		}

		public static bool IsProbeRange(RangeHeaderValue range)
		{
			if (range == null || range.Unit != "bytes" || range.Ranges.Count != 1)
				return false;
			var item = range.Ranges.First();
			return item.From == 0 && item.To <= 1;
		}
	}
}
