using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

		public AdminController(IPostService postService, IUserService userService, IImageService imageService)
		{
			_postService = postService;
			_userService = userService;
			_imageService = imageService;
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
			var post = new Post {TimeStamp = DateTime.UtcNow, IsLive = true, Name = User.Identity.Name};
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
		public async Task<IActionResult> AddImageFolder(ImageFolder imageFolder)
		{
			await _imageService.CreateImageFolder(imageFolder);
			return new EmptyResult();
		}

		[HttpPost("/Admin/DeleteImageFolder")]
		public async Task<IActionResult> DeleteImageFolder(int imageFolderID)
		{
			await _imageService.DeleteImageFolder(imageFolderID);
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
	}
}