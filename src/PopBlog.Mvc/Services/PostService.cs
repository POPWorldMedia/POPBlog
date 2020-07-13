using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IPostService
	{
		Task<IEnumerable<Post>> GetLast20LiveAndPublic();
		Task Create(Post post);
		Task Update(Post post);
		string MakeUrlString(string text);
		Task<IEnumerable<Post>> GetLast20();
		Task<Post> Get(int postID);
		Task<Post> Get(string urlTitle);
		Task<IEnumerable<MonthCount>> GetArchiveCounts();
		Task<IEnumerable<Post>> GetPostsByMonth(int year, int month);
	}

	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IConfig _config;
		private readonly ITimeAdjustService _timeAdjustService;

		public PostService(IPostRepository postRepository, IConfig config, ITimeAdjustService timeAdjustService)
		{
			_postRepository = postRepository;
			_config = config;
			_timeAdjustService = timeAdjustService;
		}

		public async Task<IEnumerable<Post>> GetLast20LiveAndPublic()
		{
			var posts = await _postRepository.GetLast20LiveAndPublic();
			var list = posts.ToArray();
			foreach (var item in list)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return list;
		}

		public async Task Create(Post post)
		{
			post.TimeStamp = _timeAdjustService.GetReverseAdjustedTime(post.TimeStamp);
			await SetUrlTitle(post);
			await _postRepository.Create(post);
		}

		public async Task Update(Post post)
		{
			post.TimeStamp = _timeAdjustService.GetReverseAdjustedTime(post.TimeStamp);
			if (post.PostID == 0)
				throw new ArgumentException("There is no PostID, yo. You can't update this post.");
			var originalPost = await _postRepository.Get(post.PostID);
			if (originalPost.Title != post.Title)
				await SetUrlTitle(post);
			else
				post.UrlTitle = originalPost.UrlTitle;
			await _postRepository.Update(post);
		}

		private async Task SetUrlTitle(Post post)
		{
			post.UrlTitle = MakeUrlString(post.Title);
			var matches = await _postRepository.GetPostsThatUrlStartWith(post.UrlTitle);
			var matchTest = post.UrlTitle.Replace("-", @"\-");
			var count = matches.Count(p => Regex.IsMatch(p.UrlTitle, @"^(" + matchTest + @")(\-\d)?$"));
			if (count > 0)
				post.UrlTitle = post.UrlTitle + "-" + count;
		}

		public string MakeUrlString(string text)
		{
			if (text == null)
				throw new Exception("Can't Url convert a null string.");
			var result = text.Replace(" ", "-");
			var replacer = new Regex(@"[^\w\-]", RegexOptions.Compiled);
			result = replacer.Replace(result, "").ToLower();
			return result;
		}

		public async Task<IEnumerable<Post>> GetLast20()
		{
			var posts = await _postRepository.GetLast20();
			var list = posts.ToArray();
			foreach (var item in list)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return list;
		}

		public async Task<Post> Get(int postID)
		{
			var post = await _postRepository.Get(postID);
			post.TimeStamp = _timeAdjustService.GetAdjustedTime(post.TimeStamp);
			return post;
		}

		public async Task<Post> Get(string urlTitle)
		{
			var post = await _postRepository.Get(urlTitle);
			post.TimeStamp = _timeAdjustService.GetAdjustedTime(post.TimeStamp);
			return post;
		}

		public async Task<IEnumerable<MonthCount>> GetArchiveCounts()
		{
			return await _postRepository.GetArchiveCounts();
		}

		public async Task<IEnumerable<Post>> GetPostsByMonth(int year, int month)
		{
			var start = _timeAdjustService.GetReverseAdjustedTime(new DateTime(year, month, 1));
			var end = _timeAdjustService.GetReverseAdjustedTime(new DateTime(year, month, 1).AddMonths(1));
			var posts = await _postRepository.GetByDateRange(start, end);
			var list = posts.ToArray();
			foreach (var item in list)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return list;
		}
	}
}