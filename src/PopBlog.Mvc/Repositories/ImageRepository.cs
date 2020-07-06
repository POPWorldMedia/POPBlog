using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface IImageRepository
	{
		Task RemoveImagesFromFolder(int imageFolderID);
		Task<IEnumerable<Image>> GetImagesByFolder(int? imageFolderID);
	}

	public class ImageRepository : IImageRepository
	{
		private readonly IConfig _config;

		public ImageRepository(IConfig config)
		{
			_config = config;
		}

		public async Task RemoveImagesFromFolder(int imageFolderID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE [Images] SET ImageFolderID = null WHERE ImageFolderID = @ImageFolderID", new { ImageFolderID = imageFolderID });
		}

		public async Task<IEnumerable<Image>> GetImagesByFolder(int? imageFolderID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var images = await connection.QueryAsync<Image>("SELECT ImageID, MimeType, FileName, ImageFolderID, TimeStamp FROM Images WHERE ImageFolderID = @ImageFolderID", new {ImageFolderID = imageFolderID});
			return images;
		}
	}
}