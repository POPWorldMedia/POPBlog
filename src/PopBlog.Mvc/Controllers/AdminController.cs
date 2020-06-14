using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PopBlog.Mvc.Controllers
{
	[Authorize(Policy = "Admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class AdminController : Controller
	{
		public AdminController()
		{
		}

		[HttpGet("/admin")]
		public async Task<IActionResult> Index()
		{
			return View();
		}
	}
}