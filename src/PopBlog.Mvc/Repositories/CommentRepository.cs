using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface ICommentRepository
	{
		Task<IEnumerable<Comment>> GetRecent();
		Task Create(Comment comment);
		Task Delete(int commentID);
		Task<IEnumerable<Comment>> GetByPost(int postID);
	}

	public class CommentRepository : ICommentRepository
	{
		private readonly IConfig _config;

		public CommentRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<IEnumerable<Comment>> GetRecent()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var comments = await connection.QueryAsync<Comment>("SELECT TOP 30 * FROM Comments ORDER BY TimeStamp DESC");
			return comments;
		}

		public async Task Create(Comment comment)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO Comments (PostID, UserID, FullText, TimeStamp, IsLive, IP, Name, Email, WebSite) VALUES (@PostID, @UserID, @FullText, @TimeStamp, @IsLive, @IP, @Name, @Email, @WebSite)", comment);
		}

		public async Task Delete(int commentID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("DELETE FROM Comments WHERE CommentID = @CommentID", new {CommentID = commentID});
		}

		public async Task<IEnumerable<Comment>> GetByPost(int postID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var comments = await connection.QueryAsync<Comment>("SELECT * FROM Comments WHERE PostID = @PostID ORDER BY TimeStamp", new {PostID = postID});
			return comments;
		}
	}
}