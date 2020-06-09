using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PopBlog.Mvc.Controllers
{
	public class HomeController : Controller
	{
		[HttpGet("/")]
		public async Task<IActionResult> Index()
		{
			return View();
		}
	}
}
