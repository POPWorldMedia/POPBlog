using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface ICommentService
	{
		Task<IEnumerable<Comment>> GetRecent();
		Task Delete(int commentID);
		Task Create(int postID, string fullText, string name, string email, string webSite);
	}

	public class CommentService : ICommentService
	{
		private readonly ICommentRepository _commentRepository;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly IUserRepository _userRepository;
		private readonly ITextParsingService _textParsingService;

		public CommentService(ICommentRepository commentRepository, IHttpContextAccessor contextAccessor, IUserRepository userRepository, ITextParsingService textParsingService)
		{
			_commentRepository = commentRepository;
			_contextAccessor = contextAccessor;
			_userRepository = userRepository;
			_textParsingService = textParsingService;
		}

		public async Task<IEnumerable<Comment>> GetRecent()
		{
			return await _commentRepository.GetRecent();
		}

		public async Task Delete(int commentID)
		{
			await _commentRepository.Delete(commentID);
		}

		public async Task Create(int postID, string fullText, string name, string email, string webSite)
		{
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
			await _commentRepository.Create(comment);
		}
	}
}