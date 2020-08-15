using System;
using Microsoft.Extensions.Configuration;

namespace PopBlog.Mvc
{
	public interface IConfig
	{
		string ConnectionString { get; }
		string ReCaptchaSiteKey { get; }
		string ReCaptchaSecretKey { get; }
		string BlogTitle { get; }
		string BlogDescription { get; }
		double TimeZone { get; }
		string StorageConnectionString { get; }
		string StorageContainerName { get; }
		string StorageAccountBaseUrl { get; }
		string Language { get; }
		string FeedImageUrl { get; }
		string ItunesCategory { get; }
		bool? ItunesExplicit { get; }
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
		public string BlogTitle => _configuration["BlogTitle"];
		public string BlogDescription => _configuration["BlogDescription"];
		public double TimeZone => Convert.ToDouble(_configuration["TimeZone"]);
		public string StorageConnectionString => _configuration["StorageConnectionString"];
		public string StorageContainerName => _configuration["StorageContainerName"];
		public string StorageAccountBaseUrl => _configuration["StorageAccountBaseUrl"];
		public string Language => _configuration["Language"] ?? "en";
		public string FeedImageUrl => _configuration["FeedImageUrl"];
		public string ItunesCategory => _configuration["ItunesCategory"];
		public bool? ItunesExplicit => Convert.ToBoolean(_configuration["ItunesExplicit"]);
	}
}