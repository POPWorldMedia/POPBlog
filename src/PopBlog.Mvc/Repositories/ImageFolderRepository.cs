using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface IImageFolderRepository
	{
		Task<IEnumerable<ImageFolder>> GetAll();
		Task Create(ImageFolder imageFolder);
		Task Delete(int imageFolderID);
	}

	public class ImageFolderRepository : IImageFolderRepository
	{
		private readonly IConfig _config;

		public ImageFolderRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<IEnumerable<ImageFolder>> GetAll()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<ImageFolder>("SELECT * FROM [ImageFolders] ORDER BY [Name]");
			return list.ToArray();
		}

		public async Task Create(ImageFolder imageFolder)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO ImageFolders (Name, ParentImageFolderID) VALUES (@Name, @ParentImageFolderID)", imageFolder);
		}

		public async Task Delete(int imageFolderID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("DELETE FROM ImageFolders WHERE ImageFolderID = @ImageFolderID", new {ImageFolderID = imageFolderID});
		}
	}
}