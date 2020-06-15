using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IPostService
	{
		Task Create(Post post);
		string MakeUrlString(string text);
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

		public async Task Create(Post post)
		{
			//post.TimeStamp = _timeAdjust.GetReverseAdjustedTime(_config.GetTimeZoneAdjustment(), post.TimeStamp);
			post.UrlTitle = MakeUrlString(post.Title);
			var matches = await _postRepository.GetPostsThatUrlStartWith(post.UrlTitle);
			var matchTest = post.UrlTitle.Replace("-", @"\-");
			var count = matches.Count(p => Regex.IsMatch(p.UrlTitle, @"^(" + matchTest + @")(\-\d)?$"));
			if (count > 0)
				post.UrlTitle = post.UrlTitle + "-" + count;
			await _postRepository.Create(post);
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
	}
}