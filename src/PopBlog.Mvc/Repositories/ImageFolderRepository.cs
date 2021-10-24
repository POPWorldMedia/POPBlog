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
		Task<int> Create(ImageFolder imageFolder);
		Task Delete(int imageFolderID);
		Task<int> GetPhotoPostFolderID();
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

		public async Task<int> Create(ImageFolder imageFolder)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var imageFolderID = await connection.QuerySingleAsync<int>("INSERT INTO ImageFolders (Name, ParentImageFolderID) VALUES (@Name, @ParentImageFolderID);SELECT CAST(SCOPE_IDENTITY() as int)", imageFolder);
			return imageFolderID;
		}

		public async Task Delete(int imageFolderID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("DELETE FROM ImageFolders WHERE ImageFolderID = @ImageFolderID", new {ImageFolderID = imageFolderID});
		}

		public async Task<int> GetPhotoPostFolderID()
		{
			const string folderName = "Photo Posts";
			var folders = await GetAll();
			var folder = folders.SingleOrDefault(x => x.Name == folderName);
			if (folder == null)
			{
				var imageFolder = new ImageFolder { Name = folderName };
				imageFolder.ImageFolderID = await Create(imageFolder);
				folder = imageFolder;
			}
			return folder.ImageFolderID;
		}
	}
}