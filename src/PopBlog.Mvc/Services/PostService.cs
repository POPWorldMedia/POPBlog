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
		Task<List<Post>> GetLast20LiveAndPublic();
		Task Create(Post post);
		Task Update(Post post);
		string MakeUrlString(string text);
		Task<List<Post>> GetLast20();
		Task<Post> Get(int postID);
		Task<Post> Get(string urlTitle);
	}

	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IConfig _config;

		public PostService(IPostRepository postRepository, IConfig config)
		{
			_postRepository = postRepository;
			_config = config;
		}

		public async Task<List<Post>> GetLast20LiveAndPublic()
		{
			return await _postRepository.GetLast20LiveAndPublic();
		}

		public async Task Create(Post post)
		{
			//post.TimeStamp = _timeAdjust.GetReverseAdjustedTime(_config.GetTimeZoneAdjustment(), post.TimeStamp);
			await SetUrlTitle(post);
			await _postRepository.Create(post);
		}

		public async Task Update(Post post)
		{
			//post.TimeStamp = _timeAdjust.GetReverseAdjustedTime(_config.GetTimeZoneAdjustment(), post.TimeStamp);
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

		public async Task<List<Post>> GetLast20()
		{
			return await _postRepository.GetLast20();
		}

		public async Task<Post> Get(int postID)
		{
			return await _postRepository.Get(postID);
		}

		public async Task<Post> Get(string urlTitle)
		{
			return await _postRepository.Get(urlTitle);
		}
	}
}