using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Controllers
{
	public class ContentController : Controller
	{
		private readonly IContentService _contentService;

		public ContentController(IContentService contentService)
		{
			_contentService = contentService;
		}

		[HttpGet("/content/{id}")]
		public async Task<IActionResult> Detail(string id)
		{
			var content = await _contentService.Get(id);
			if (content == null)
				return StatusCode(404);
			return View(content);
		}
	}
}