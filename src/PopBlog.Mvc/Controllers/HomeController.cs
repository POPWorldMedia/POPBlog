using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private readonly IUserService _userService;
		private readonly IReCaptchaService _reCaptchaService;

		public HomeController(IUserService userService, IReCaptchaService reCaptchaService)
		{
			_userService = userService;
			_reCaptchaService = reCaptchaService;
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

		[HttpGet("/error")]
		public ViewResult Error()
		{
			return View();
		}

		[HttpGet("/login")]
		public ViewResult Login()
		{
			return View();
		}

		[HttpPost("/login")]
		public async Task<IActionResult> Login(string email, string password, string token)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			var reCaptchaResult = await _reCaptchaService.VerifyToken(token, ip);
			if (!reCaptchaResult.IsSuccess)
			{
				ModelState.AddModelError("token", $"I don't think you're for real. {reCaptchaResult.ErrorCodes.FirstOrDefault()}");
				return View();
			}
			if (await _userService.IsValidUser(email, password))
			{
				var user = await _userService.GetUserByEmail(email);
				if (user == null)
					throw new Exception($"Weird, there was no user with the email {email}.");
				var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Role, "Admin") };

				var props = new AuthenticationProperties { IsPersistent = true };
				props.ExpiresUtc = DateTime.UtcNow.AddYears(1);

				var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);
				return RedirectToAction("Index");
			}
			ModelState.AddModelError("email", "Login incorrect");
			return View();
		}

		[HttpGet("/logout")]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index");
		}
	}
}
