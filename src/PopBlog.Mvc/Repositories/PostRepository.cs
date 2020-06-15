using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface IPostRepository
	{
		Task Create(Post post);
		Task<List<Post>> GetPostsThatUrlStartWith(string urlTitle);
	}

	public class PostRepository : IPostRepository
	{
		private readonly IConfig _config;

		public PostRepository(IConfig config)
		{
			_config = config;
		}

		public async Task Create(Post post)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO Posts (UserID, Title, FullText, TimeStamp, Replies, IsPrivate, IsClosed, IsLive, UrlTitle) VALUES (@UserID, @Title, @FullText, @TimeStamp, @Replies, @IsPrivate, @IsClosed, @IsLive, @UrlTitle)", post);
		}

		public async Task<List<Post>> GetPostsThatUrlStartWith(string urlTitle)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT * FROM Posts WHERE UrlTitle LIKE @UrlTitle + '%'", new {UrlTitle = urlTitle});
			return list.ToList();
		}
	}
}