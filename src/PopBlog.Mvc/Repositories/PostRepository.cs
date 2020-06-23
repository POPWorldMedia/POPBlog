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
		Task Update(Post post);
		Task<List<Post>> GetPostsThatUrlStartWith(string urlTitle);
		Task<List<Post>> GetLast20();
		Task<List<Post>> GetLast20LiveAndPublic();
		Task<Post> Get(int postID);
		Task<Post> Get(string urlTitle);
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
			await connection.ExecuteAsync("INSERT INTO Posts (UserID, Title, FullText, TimeStamp, Replies, IsPrivate, IsClosed, IsLive, UrlTitle, Name) VALUES (@UserID, @Title, @FullText, @TimeStamp, @Replies, @IsPrivate, @IsClosed, @IsLive, @UrlTitle, @Name)", post);
		}

		public async Task Update(Post post)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE Posts SET Title = @Title, FullText = @FullText, TimeStamp = @TimeStamp, IsPrivate = @IsPrivate, IsClosed = @IsClosed, IsLive = @IsLive, UrlTitle = @UrlTitle, Name = @Name WHERE PostID = @PostID", post);
		}

		public async Task<List<Post>> GetPostsThatUrlStartWith(string urlTitle)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT * FROM Posts WHERE UrlTitle LIKE @UrlTitle + '%'", new {UrlTitle = urlTitle});
			return list.ToList();
		}

		public async Task<List<Post>> GetLast20()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT TOP 20 * FROM Posts ORDER BY TimeStamp DESC");
			return list.ToList();
		}

		public async Task<List<Post>> GetLast20LiveAndPublic()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT TOP 20 * FROM Posts WHERE IsLive = 1 AND IsPrivate = 0 ORDER BY TimeStamp DESC");
			return list.ToList();
		}

		public async Task<Post> Get(int postID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var post = await connection.QuerySingleOrDefaultAsync<Post>("SELECT * FROM Posts WHERE PostID = @PostID", new {PostID = postID});
			return post;
		}

		public async Task<Post> Get(string urlTitle)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var post = await connection.QuerySingleOrDefaultAsync<Post>("SELECT * FROM Posts WHERE UrlTitle = @UrlTitle", new { UrlTitle = urlTitle });
			return post;
		}
	}
}