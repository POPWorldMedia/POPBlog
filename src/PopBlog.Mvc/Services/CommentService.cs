using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface ICommentService
	{
		Task<IEnumerable<CommentLinkContainer>> GetRecent();
		Task Delete(int commentID);
		Task<int> Create(int postID, string fullText, string name, string email, string webSite);
		Task<IEnumerable<Comment>> GetByPost(int postID);
	}

	public class CommentService : ICommentService
	{
		private readonly ICommentRepository _commentRepository;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly IUserRepository _userRepository;
		private readonly ITextParsingService _textParsingService;
		private readonly IPostRepository _postRepository;
		private readonly ITimeAdjustService _timeAdjustService;

		public CommentService(ICommentRepository commentRepository, IHttpContextAccessor contextAccessor, IUserRepository userRepository, ITextParsingService textParsingService, IPostRepository postRepository, ITimeAdjustService timeAdjustService)
		{
			_commentRepository = commentRepository;
			_contextAccessor = contextAccessor;
			_userRepository = userRepository;
			_textParsingService = textParsingService;
			_postRepository = postRepository;
			_timeAdjustService = timeAdjustService;
		}

		public async Task<IEnumerable<CommentLinkContainer>> GetRecent()
		{
			return await _commentRepository.GetRecentCommentLinkContainers();
		}

		public async Task Delete(int commentID)
		{
			var comment = await _commentRepository.Get(commentID);
			await _commentRepository.Delete(commentID);
			await UpdateReplyCount(comment.PostID);
		}

		public async Task<int> Create(int postID, string fullText, string name, string email, string webSite)
		{
			var post = await _postRepository.Get(postID);
			if (post == null || post.IsClosed)
				return 0;
			int? userID = null;
			if (_contextAccessor.HttpContext.User?.Identity?.Name != null)
			{
				var user = await _userRepository.GetUserByName(_contextAccessor.HttpContext.User.Identity.Name);
				name = user.Name;
				email = user.Email;
				userID = user.UserID;
				webSite = "/";
				fullText = _textParsingService.UnrestrictedParse(fullText);
			}
			else
			{
				fullText = _textParsingService.CommentParse(fullText);
			}
			if (string.IsNullOrEmpty(name))
				name = "Anonymous";
			var ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
			var timeStamp = DateTime.UtcNow;
			var comment = new Comment
			{
				PostID = postID,
				FullText = fullText,
				Name = name,
				Email = email,
				UserID = userID,
				WebSite = webSite,
				IP = ip,
				TimeStamp = timeStamp,
				IsLive = true
			};
			var id = await _commentRepository.Create(comment);
			await UpdateReplyCount(post.PostID);
			return id;
		}

		private async Task UpdateReplyCount(int postID)
		{
			var count = await _commentRepository.GetCommentCount(postID);
			await _postRepository.UpdateReplies(postID, count);
		}

		public async Task<IEnumerable<Comment>> GetByPost(int postID)
		{
			var comments = await _commentRepository.GetByPost(postID);
			var commentList = comments.ToList();
			foreach (var item in commentList)
				item.TimeStamp = _timeAdjustService.GetAdjustedTime(item.TimeStamp);
			return commentList;
		}
	}
}