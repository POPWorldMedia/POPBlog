using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Services;

namespace PopBlog.Mvc.Repositories
{
	public interface IPostRepository
	{
		Task Create(Post post);
		Task Update(Post post);
		Task<IEnumerable<Post>> GetPostsThatUrlStartWith(string urlTitle);
		Task<IEnumerable<Post>> GetLast20();
		Task<IEnumerable<Post>> GetLast20LiveAndPublic();
		Task<Post> Get(int postID);
		Task<Post> Get(string urlTitle);
		Task<IEnumerable<MonthCount>> GetArchiveCounts();
		Task<IEnumerable<Post>> GetByDateRange(DateTime start, DateTime end);
		Task UpdateReplies(int postID, int count);
		Task IncrementDownloadCount(int postID);
		Task Delete(int postID);
	}

	public class PostRepository : IPostRepository
	{
		private readonly IConfig _config;
		private readonly ITimeAdjustService _timeAdjustService;

		public PostRepository(IConfig config, ITimeAdjustService timeAdjustService)
		{
			_config = config;
			_timeAdjustService = timeAdjustService;
		}

		public async Task Create(Post post)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO Posts (UserID, Title, FullText, TimeStamp, Replies, IsPrivate, IsClosed, IsLive, UrlTitle, Name, IsPodcastPost, FileName, DownloadCount, Length, Size) VALUES (@UserID, @Title, @FullText, @TimeStamp, @Replies, @IsPrivate, @IsClosed, @IsLive, @UrlTitle, @Name, @IsPodcastPost, @FileName, @DownloadCount, @Length, @Size)", post);
		}

		public async Task Update(Post post)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE Posts SET Title = @Title, FullText = @FullText, TimeStamp = @TimeStamp, IsPrivate = @IsPrivate, IsClosed = @IsClosed, IsLive = @IsLive, UrlTitle = @UrlTitle, Name = @Name, IsPodcastPost = @IsPodcastPost, FileName = @FileName, DownloadCount = @DownloadCount, Length = @Length, Size = @Size WHERE PostID = @PostID", post);
		}

		public async Task<IEnumerable<Post>> GetPostsThatUrlStartWith(string urlTitle)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT * FROM Posts WHERE UrlTitle LIKE @UrlTitle + '%'", new {UrlTitle = urlTitle});
			return list.ToList();
		}

		public async Task<IEnumerable<Post>> GetLast20()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT TOP 20 * FROM Posts ORDER BY TimeStamp DESC");
			return list.ToList();
		}

		public async Task<IEnumerable<Post>> GetLast20LiveAndPublic()
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

		public async Task<IEnumerable<MonthCount>> GetArchiveCounts()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<DateTime>("SELECT TimeStamp FROM Posts WHERE IsLive = 1 AND IsPrivate = 0 ORDER BY TimeStamp");
			var newList = list.Select(x => _timeAdjustService.GetAdjustedTime(x))
				.GroupBy(x => new {x.Month, x.Year})
				.Select(x => new MonthCount{Count = x.Count(), Year = x.Key.Year, Month = x.Key.Month})
				.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);
			return newList;
		}

		public async Task<IEnumerable<Post>> GetByDateRange(DateTime start, DateTime end)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<Post>("SELECT * FROM Posts WHERE TimeStamp >= @Start AND TimeStamp < @End AND IsLive = 1 AND IsPrivate = 0 ORDER BY TimeStamp DESC", new {Start = start, End = end});
			return list;
		}

		public async Task UpdateReplies(int postID, int count)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE Posts SET Replies = @Replies WHERE PostID = @PostID", new {PostID = postID, Replies = count});
		}

		public async Task IncrementDownloadCount(int postID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("UPDATE Posts SET DownloadCount = DownloadCount + 1 WHERE PostID = @PostID", new { PostID = postID });
		}

		public async Task Delete(int postID)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("DELETE FROM Posts WHERE PostID = @PostID", new { PostID = postID });
		}
	}
}