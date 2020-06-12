using System;
using System.Security.Cryptography;
using System.Text;

namespace PopBlog.Mvc.Extensions
{
	public static class Strings
	{
		public static string GetSHA256Hash(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			var input = Encoding.UTF8.GetBytes(text);
			using (var sha256 = SHA256.Create())
			{
				var output = sha256.ComputeHash(input);
				return Convert.ToBase64String(output);
			}
		}
	}
}