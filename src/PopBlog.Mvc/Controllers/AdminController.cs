using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	[Authorize(Policy = "Admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class AdminController : Controller
	{
		private readonly IPostService _postService;
		private readonly IUserService _userService;
		private readonly IImageService _imageService;
		private readonly ITimeAdjustService _timeAdjustService;
		private readonly IContentService _contentService;
		private readonly ICommentService _commentService;
		private readonly IIpBanService _ipBanService;

		public AdminController(IPostService postService, IUserService userService, IImageService imageService, ITimeAdjustService timeAdjustService, IContentService contentService, ICommentService commentService, IIpBanService ipBanService)
		{
			_postService = postService;
			_userService = userService;
			_imageService = imageService;
			_timeAdjustService = timeAdjustService;
			_contentService = contentService;
			_commentService = commentService;
			_ipBanService = ipBanService;
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
			var currentTime = _timeAdjustService.GetAdjustedTime(DateTime.UtcNow);
			var post = new Post {TimeStamp = currentTime, IsLive = true, Name = User.Identity.Name};
			return View(post);
		}

		[HttpGet("/admin/newphotopost")]
		public IActionResult NewPhotoPost()
		{
			return View();
		}

		[DisableRequestSizeLimit]
		[HttpPost("/admin/newpost")]
		public async Task<IActionResult> NewPost(Post post, IFormFile podcastFile)
		{
			var user = await _userService.GetUserByName(User.Identity.Name);
			post.UserID = user.UserID;
			await _postService.Create(post, podcastFile?.FileName, podcastFile?.OpenReadStream());
			return RedirectToAction("Index", "Admin");
		}

		[DisableRequestSizeLimit]
		[HttpPost("/admin/newphotopost")]
		public async Task<IActionResult> NewPhotoPost(Post post, IFormFile photoFile)
		{
			var user = await _userService.GetUserByName(User.Identity.Name);
			post.UserID = user.UserID;
			post.Name = user.Name;
			post.TimeStamp = _timeAdjustService.GetAdjustedTime(DateTime.UtcNow);
			post.IsLive = true;
			string ImagePathMaker(int id) => Url.Action("Image", "Post", new {id});
			var stream = photoFile.OpenReadStream();
			var length = (int)stream.Length;
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			await _postService.CreatePhotoPost(post, photoFile.FileName, bytes, ImagePathMaker);
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

		[DisableRequestSizeLimit]
		[HttpPost("/admin/editpost/{postID}")]
		public async Task<IActionResult> EditPost(int postID, Post post, IFormFile podcastFile)
		{
			post.PostID = postID;
			await _postService.Update(post, podcastFile?.FileName, podcastFile?.OpenReadStream());
			return RedirectToAction("Index", "Admin");
		}

		[HttpPost("/admin/deletepost")]
		public async Task<IActionResult> DeletePost(int postID)
		{
			await _postService.Delete(postID);
			return RedirectToAction("Index");
		}

		[HttpGet("/Admin/ImageManager")]
		public async Task<IActionResult> ImageManager()
		{
			await SetupFolders();
			return View();
		}

		private async Task SetupFolders()
		{
			var folders = await _imageService.GetAllImageFolders();
			var list = folders.ToList();
			list.Insert(0, new ImageFolder {ImageFolderID = 0, Name = "Root Folder"});
			ViewBag.Folders = list;
		}

		[HttpPost("/Admin/ImageManager")]
		public async Task<IActionResult> ImageManager(IFormFile imageUpload)
		{
			var file = imageUpload;
			var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim().ToString();
			int? imageFolderID = null;
			if (!string.IsNullOrEmpty(Request.Form["FolderList"]) && Request.Form["FolderList"] != "0")
				imageFolderID = Convert.ToInt32(Request.Form["FolderList"]);
			byte[] bytes = new byte[(int)file.Length];
			await file.OpenReadStream().ReadAsync(bytes, 0, (int)file.Length);
			string mimeType;
			var ext = System.IO.Path.GetExtension(fileName);
			switch (ext)
			{
				case ".jpg":
				case ".jpeg":
					mimeType = "image/jpeg";
					break;
				case ".gif":
					mimeType = "image/gif";
					break;
				case ".png":
					mimeType = "image/png";
					break;
				default:
					throw new Exception("File upload type unknown.");
			}
			var newImageID = await _imageService.CreateImage(bytes, fileName, mimeType, imageFolderID);
			ViewBag.NewImageID = newImageID;
			await SetupFolders();
			return View();
		}

		[HttpGet("/Admin/ImageFolderManager")]
		public IActionResult ImageFolderManager()
		{
			return View();
		}

		[HttpGet("/Admin/GetFolderList")]
		public async Task<IActionResult> GetFolderList()
		{
			var list = await _imageService.GetAllImageFolders();
			return Json(list);
		}

		[HttpPost("/Admin/AddImageFolder")]
		public async Task<IActionResult> AddImageFolder([FromBody] ImageFolder imageFolder)
		{
			await _imageService.CreateImageFolder(imageFolder);
			return new EmptyResult();
		}

		[HttpPost("/Admin/DeleteImageFolder")]
		public async Task<IActionResult> DeleteImageFolder([FromBody] ImageFolder imageFolder)
		{
			await _imageService.DeleteImageFolder(imageFolder.ImageFolderID);
			return new EmptyResult();
		}

		[HttpGet("/Admin/GetImages/{id}")]
		public async Task<IActionResult> GetImages(int id)
		{
			int? imageFolderID = id == 0 ? (int?)null : id;
			var list = await _imageService.GetImagesByFolder(imageFolderID);
			return Json(list);
		}

		[HttpGet("/Admin/GetImageInfo/{id}")]
		public async Task<IActionResult> GetImageInfo(int id)
		{
			var image = await _imageService.GetImage(id);
			return Json(image);
		}

		[HttpGet("/admin/content")]
		public async Task<IActionResult> Content()
		{
			var content = await _contentService.GetAll();
			foreach (var item in content)
				item.LastUpdated = _timeAdjustService.GetAdjustedTime(item.LastUpdated);
			return View(content);
		}

		[HttpPost("/admin/newcontent")]
		public async Task<IActionResult> NewContent(Content content)
		{
			await _contentService.Create(content);
			return RedirectToAction("EditContent", new {id = content.ContentID});
		}

		[HttpGet("/admin/editcontent/{id}")]
		public async Task<IActionResult> EditContent(string id)
		{
			var content = await _contentService.Get(id);
			if (content == null)
				return StatusCode(404);
			return View(content);
		}

		[HttpPost("/admin/editcontent/{id}")]
		public async Task<IActionResult> EditContent(string id, Content content)
		{
			await _contentService.Update(id, content);
			return RedirectToAction("Content");
		}

		[HttpPost("/admin/deletecomment/{id}")]
		public async Task<IActionResult> DeleteComment(int id)
		{
			await _commentService.Delete(id);
			return RedirectToAction("Comments");
		}

		[HttpGet("/admin/comments")]
		public async Task<IActionResult> Comments()
		{
			var comments = await _commentService.GetRecent();
			foreach (var item in comments)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return View(comments);
		}

		[HttpGet("/admin/ipban")]
		public async Task<ActionResult> IPBan()
		{
			var list = await _ipBanService.GetAll();
			return View(list);
		}

		[HttpPost("/admin/addipban")]
		public async Task<ActionResult> AddIPBan(string newIP)
		{
			if (!string.IsNullOrEmpty(newIP))
				await _ipBanService.Add(newIP);
			return RedirectToAction("IPBan");
		}

		[HttpPost("/admin/deleteipban")]
		public async Task<ActionResult> DeleteIPBan(string banList)
		{
			if (!string.IsNullOrEmpty(banList))
				await _ipBanService.Delete(banList);
			return RedirectToAction("IPBan");
		}

		[HttpGet("/admin/users")]
		public async Task<ActionResult> Users()
		{
			var users = await _userService.GetAll();
			return View(users);
		}

		[HttpPost("/admin/deleteuser")]
		public async Task<ActionResult> DeleteUser(int id)
		{
			await _userService.Delete(id);
			return RedirectToAction("Users");
		}

		[HttpPost("/admin/newuser")]
		public async Task<ActionResult> NewUser(User user)
		{
			await _userService.Create(user);
			return RedirectToAction("Users");
		}

		[HttpGet("/admin/edituser/{id}")]
		public async Task<ActionResult> EditUser(int id)
		{
			var user = await _userService.Get(id);
			return View(user);
		}

		[HttpPost("/admin/edituser/{id}")]
		public async Task<ActionResult> EditUser(int id, User user)
		{
			await _userService.Update(id, user.Name, user.Email, user.Password);
			return RedirectToAction("Users");
		}
	}
}