using Microsoft.Extensions.DependencyInjection;
using PopBlog.Mvc.Repositories;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Extensions
{
	public static class ServiceCollections
	{
		public static IServiceCollection AddPopBlogServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<IConfig, Config>();

			serviceCollection.AddTransient<IUserRepository, UserRepository>();
			serviceCollection.AddTransient<IPostRepository, PostRepository>();
			serviceCollection.AddTransient<IImageFolderRepository, ImageFolderRepository>();
			serviceCollection.AddTransient<IImageRepository, ImageRepository>();

			serviceCollection.AddTransient<IUserService, UserService>();
			serviceCollection.AddTransient<IReCaptchaService, ReCaptchaService>();
			serviceCollection.AddTransient<IPostService, PostService>();
			serviceCollection.AddTransient<IImageService, ImageService>();

			return serviceCollection;
		}
	}
}