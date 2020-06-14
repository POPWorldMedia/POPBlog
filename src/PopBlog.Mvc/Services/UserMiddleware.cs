using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace PopBlog.Mvc.Services
{
	public class UserMiddleware
	{
		private readonly RequestDelegate _next;

		public UserMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task InvokeAsync(HttpContext context, IUserService userService)
		{
			var authResult = context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
			if (authResult?.Principal?.Identity is ClaimsIdentity identity)
			{
				context.User = new ClaimsPrincipal(identity);
			}
			return _next.Invoke(context);
		}
	}
}