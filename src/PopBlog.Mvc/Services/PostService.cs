using System;
using System.Collections.Generic;
using System.IO;
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
		Task Create(Post post, string fileName, Stream stream);
		Task Update(Post post, string fileName, Stream stream);
		string MakeUrlString(string text);
		Task<IEnumerable<Post>> GetLast20();
		Task<Post> Get(int postID);
		Task<Post> Get(string urlTitle);
		Task<IEnumerable<MonthCount>> GetArchiveCounts();
		Task<IEnumerable<Post>> GetPostsByMonth(int year, int month);
		Task<string> GetDownloadLink(int postID);
		Task IncrementDownloadCount(int postID);
		Task Delete(int postID);
	}

	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IConfig _config;
		private readonly ITimeAdjustService _timeAdjustService;
		private readonly ICommentRepository _commentRepository;
		private readonly IStorageRepository _storageRepository;

		public PostService(IPostRepository postRepository, IConfig config, ITimeAdjustService timeAdjustService, ICommentRepository commentRepository, IStorageRepository storageRepository)
		{
			_postRepository = postRepository;
			_config = config;
			_timeAdjustService = timeAdjustService;
			_commentRepository = commentRepository;
			_storageRepository = storageRepository;
		}

		public async Task<IEnumerable<Post>> GetLast20LiveAndPublic()
		{
			var posts = await _postRepository.GetLast20LiveAndPublic();
			var list = posts.ToArray();
			foreach (var item in list)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return list;
		}

		public async Task Create(Post post, string fileName, Stream stream)
		{
			post.TimeStamp = _timeAdjustService.GetReverseAdjustedTime(post.TimeStamp);
			if (stream != null && fileName != null)
				await ProcessUpload(post, fileName, stream);
			await SetUrlTitle(post);
			await _postRepository.Create(post);
		}

		private async Task ProcessUpload(Post post, string fileName, Stream stream)
		{
			post.FileName = fileName;
			post.IsPodcastPost = true;
			var newStream = new MemoryStream();
			await stream.CopyToAsync(newStream); // TagLib disposes of the stream so we can't save it
			var container = new FileAbstraction(fileName, newStream);
			var file = TagLib.File.Create(container);
			post.Length = file.Properties.Duration.ToString(@"h\:mm\:ss");
			post.Size = (int)stream.Length;
			await _storageRepository.Upload(fileName, stream);
		}

		private class FileAbstraction : TagLib.File.IFileAbstraction
		{
			private readonly Stream _stream;

			public FileAbstraction(string name, Stream stream)
			{
				Name = name;
				_stream = stream;
			}

			public void CloseStream(Stream stream)
			{
				_stream.Close();
			}

			public string Name { get; }
			public Stream ReadStream => _stream;
			public Stream WriteStream => _stream;
		}

		public async Task Update(Post post, string fileName, Stream stream)
		{
			post.TimeStamp = _timeAdjustService.GetReverseAdjustedTime(post.TimeStamp);
			if (post.PostID == 0)
				throw new ArgumentException("There is no PostID, yo. You can't update this post.");
			var originalPost = await _postRepository.Get(post.PostID);
			if (originalPost.Title != post.Title)
				await SetUrlTitle(post);
			else
				post.UrlTitle = originalPost.UrlTitle;
			if (stream != null && fileName != null)
				await ProcessUpload(post, fileName, stream);
			else
			{
				post.FileName = originalPost.FileName;
				post.IsPodcastPost = originalPost.IsPodcastPost;
				post.Length = originalPost.Length;
				post.Size = originalPost.Size;
			}
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

		public async Task<string> GetDownloadLink(int postID)
		{
			var post = await _postRepository.Get(postID);
			if (post == null)
				return null;
			var link = $"{_config.StorageAccountBaseUrl}/{_config.StorageContainerName}/{post.FileName}";
			return link;
		}

		public async Task IncrementDownloadCount(int postID)
		{
			await _postRepository.IncrementDownloadCount(postID);
		}

		public async Task Delete(int postID)
		{
			await _postRepository.Delete(postID);
		}
	}
}