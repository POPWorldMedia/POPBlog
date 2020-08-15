using Microsoft.AspNetCore.Http;
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
			serviceCollection.AddTransient<IContentRepository, ContentRepository>();
			serviceCollection.AddTransient<ICommentRepository, CommentRepository>();
			serviceCollection.AddTransient<IIpBanRepository, IpBanRepository>();
			serviceCollection.AddTransient<IStorageRepository, StorageRepository>();

			serviceCollection.AddTransient<IUserService, UserService>();
			serviceCollection.AddTransient<IReCaptchaService, ReCaptchaService>();
			serviceCollection.AddTransient<IPostService, PostService>();
			serviceCollection.AddTransient<IImageService, ImageService>();
			serviceCollection.AddTransient<ITimeAdjustService, TimeAdjustService>();
			serviceCollection.AddTransient<IContentService, ContentService>();
			serviceCollection.AddTransient<ICommentService, CommentService>();
			serviceCollection.AddTransient<ITextParsingService, TextParsingService>();
			serviceCollection.AddTransient<IIpBanService, IpBanService>();

			serviceCollection.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

			return serviceCollection;
		}
	}
}