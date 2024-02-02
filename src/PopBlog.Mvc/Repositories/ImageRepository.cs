using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
		Task<int> Create(byte[] bytes, Image image);
		Task<Image> GetImage(int imageID);
		Task<byte[]> GetImageData(int imageID);
		Task<ResponseStream> GetImageStream(int imageID);
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
			string sql;
			if (imageFolderID.HasValue)
				sql = "SELECT ImageID, MimeType, FileName, ImageFolderID, TimeStamp FROM Images WHERE ImageFolderID = @ImageFolderID ORDER BY FileName";
			else
				sql = "SELECT ImageID, MimeType, FileName, ImageFolderID, TimeStamp FROM Images WHERE ImageFolderID IS NULL ORDER BY FileName";
			await using var connection = new SqlConnection(_config.ConnectionString);
			var images = await connection.QueryAsync<Image>(sql, new {ImageFolderID = imageFolderID});
			return images;
		}

		public async Task<int> Create(byte[] bytes, Image image)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var imageID = connection.QuerySingleAsync<int>("INSERT INTO Images (MimeType, FileName, ImageFolderID, [TimeStamp], ImageBytes) VALUES (@MimeType, @FileName, @ImageFolderID, @TimeStamp, @ImageBytes);SELECT CAST(SCOPE_IDENTITY() as int)", new { image.MimeType, image.FileName, image.ImageFolderID, image.TimeStamp, ImageBytes = bytes });
			return await imageID;
		}

		public async Task<Image> GetImage(int imageID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var image = await connection.QuerySingleOrDefaultAsync<Image>("SELECT ImageID, MimeType, FileName, ImageFolderID, TimeStamp FROM Images WHERE ImageID = @ImageID", new { ImageID = imageID });
			return image;
		}

		[Obsolete("Use GetImageStream(int) instead.")]
		public async Task<byte[]> GetImageData(int imageID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var image = await connection.QuerySingleOrDefaultAsync<byte[]>("SELECT ImageBytes FROM Images WHERE ImageID = @ImageID", new { ImageID = imageID });
			
			return image;
		}
		
		public async Task<ResponseStream> GetImageStream(int imageID)
		{
			var connection = new SqlConnection(_config.ConnectionString);
			var command = new SqlCommand("SELECT ImageBytes FROM Images WHERE ImageID = @ImageID", connection);
			command.Parameters.AddWithValue("ImageID", imageID);
			connection.Open();
			var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
			if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
			{
				var stream = reader.GetStream(0);
				var responseStream = new ResponseStream(reader, connection, stream);
				return responseStream;
			}
			return default;
		}
	}
}