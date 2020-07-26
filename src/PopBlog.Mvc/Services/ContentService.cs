using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IContentService
	{
		Task<IEnumerable<Content>> GetAll();
		Task Create(Content content);
		Task<Content> Get(string id);
		Task Update(string id, Content content);
	}

	public class ContentService : IContentService
	{
		private readonly IContentRepository _contentRepository;
		private readonly ITimeAdjustService _timeAdjustService;

		public ContentService(IContentRepository contentRepository, ITimeAdjustService timeAdjustService)
		{
			_contentRepository = contentRepository;
			_timeAdjustService = timeAdjustService;
		}

		public async Task<IEnumerable<Content>> GetAll()
		{
			return await _contentRepository.GetAll();
		}

		public async Task Create(Content content)
		{
			content.LastUpdated = DateTime.UtcNow;
			content.FullText = string.Empty;
			await _contentRepository.Create(content);
		}

		public async Task<Content> Get(string id)
		{
			var content = await _contentRepository.Get(id);
			return content;
		}

		public async Task Update(string id, Content content)
		{
			content.LastUpdated = _timeAdjustService.GetReverseAdjustedTime(content.LastUpdated);
			await _contentRepository.Update(id, content);
		}
	}
}