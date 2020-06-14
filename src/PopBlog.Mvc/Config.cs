using Microsoft.Extensions.Configuration;

namespace PopBlog.Mvc
{
	public interface IConfig
	{
		string ConnectionString { get; }
		string ReCaptchaSiteKey { get; }
		string ReCaptchaSecretKey { get; }
	}

	public class Config : IConfig
	{
		private readonly IConfiguration _configuration;

		public Config(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string ConnectionString => _configuration["PopBlogConnectionString"];
		public string ReCaptchaSiteKey => _configuration["ReCaptchaSiteKey"];
		public string ReCaptchaSecretKey => _configuration["ReCaptchaSecretKey"];
	}
}