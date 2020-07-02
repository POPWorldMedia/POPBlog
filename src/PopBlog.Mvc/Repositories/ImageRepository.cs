using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace PopBlog.Mvc.Repositories
{
	public interface IImageRepository
	{
		Task RemoveImagesFromFolder(int imageFolderID);
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
	}
}