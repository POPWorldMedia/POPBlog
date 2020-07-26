using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface IContentRepository
	{
		Task<IEnumerable<Content>> GetAll();
		Task Create(Content content);
		Task<Content> Get(string id);
		Task Update(string id, Content content);
	}

	public class ContentRepository : IContentRepository
	{
		private readonly IConfig _config;

		public ContentRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<IEnumerable<Content>> GetAll()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var content = await connection.QueryAsync<Content>("SELECT * FROM Contents ORDER BY Title");
			return content;
		}

		public async Task Create(Content content)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO Contents (ContentID, Title, LastUpdated, FullText) VALUES (@ContentID, @Title, @LastUpdated, @FullText)", content);
		}

		public async Task<Content> Get(string id)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var content = await connection.QuerySingleOrDefaultAsync<Content>("SELECT * FROM Contents WHERE ContentID = @ContentID", new {ContentID = id});
			return content;
		}

		public async Task Update(string id, Content content)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE Contents SET ContentID = @ContentID, Title = @Title, LastUpdated = @LastUpdated, FullText = @FullText WHERE ContentID = @OldID", new { OldID = id, content.ContentID, content.LastUpdated, content.FullText, content.Title});
		}
	}
}