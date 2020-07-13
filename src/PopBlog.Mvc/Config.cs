﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.CompilerServices;

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
	}
}