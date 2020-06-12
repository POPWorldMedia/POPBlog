using Microsoft.Extensions.Configuration;

namespace PopBlog.Mvc
{
	public interface IConfig
	{
		string ConnectionString { get; }
	}

	public class Config : IConfig
	{
		private readonly IConfiguration _configuration;

		public Config(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string ConnectionString => _configuration["PopBlogConnectionString"];
	}
}