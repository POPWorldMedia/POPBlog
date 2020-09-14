using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface ICommentRepository
	{
		Task<IEnumerable<CommentLinkContainer>> GetRecentCommentLinkContainers();
		Task<int> Create(Comment comment);
		Task Delete(int commentID);
		Task<IEnumerable<Comment>> GetByPost(int postID);
		Task<int> GetCommentCount(int postID);
		Task<Comment> Get(int commentID);
	}

	public class CommentRepository : ICommentRepository
	{
		private readonly IConfig _config;

		public CommentRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<IEnumerable<CommentLinkContainer>> GetRecentCommentLinkContainers()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var comments = await connection.QueryAsync<CommentLinkContainer>("SELECT TOP 30 C.CommentID, P.UrlTitle, P.Title, C.TimeStamp, P.PostID FROM Comments C JOIN Posts P ON C.PostID = P.PostID ORDER BY C.TimeStamp DESC");
			return comments;
		}

		public async Task<int> Create(Comment comment)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var id = await connection.QuerySingleAsync<int>("INSERT INTO Comments (PostID, UserID, FullText, TimeStamp, IsLive, IP, Name, Email, WebSite) VALUES (@PostID, @UserID, @FullText, @TimeStamp, @IsLive, @IP, @Name, @Email, @WebSite);SELECT CAST(SCOPE_IDENTITY() as int)", comment);
			return id;
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

		public async Task<int> GetCommentCount(int postID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var count = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Comments WHERE PostID = @PostID", new {PostID = postID});
			return count;
		}

		public async Task<Comment> Get(int commentID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var comment = await connection.QuerySingleOrDefaultAsync<Comment>("SELECT * FROM Comments WHERE CommentID = @CommentID", new {CommentID = commentID});
			return comment;
		}
	}
}