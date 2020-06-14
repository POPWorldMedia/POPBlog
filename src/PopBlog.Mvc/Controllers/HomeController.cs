using System;
using System.Collections.Generic;
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

		public HomeController(IUserService userService)
		{
			_userService = userService;
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
		public async Task<IActionResult> Login(string email, string password, bool rememberMe)
		{
			if (await _userService.IsValidUser(email, password))
			{
				var user = await _userService.GetUserByEmail(email);
				if (user == null)
					throw new Exception($"Weird, there was no user with the email {email}.");
				var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Role, "Admin") };

				var props = new AuthenticationProperties { IsPersistent = rememberMe };
				if (rememberMe)
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
