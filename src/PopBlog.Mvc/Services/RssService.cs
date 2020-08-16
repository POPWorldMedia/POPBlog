using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IRssService
	{
		Task<XDocument> GetMainFeed();
	}

	public class RssService : IRssService
	{
		private readonly IPostRepository _postRepository;
		private readonly IConfig _config;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly XNamespace _atom = "http://www.w3.org/2005/Atom";
		private readonly XNamespace _itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";

		public RssService(IPostRepository postRepository, IConfig config, IHttpContextAccessor httpContextAccessor)
		{
			_postRepository = postRepository;
			_config = config;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<XDocument> GetMainFeed()
		{
			var title = _config.BlogTitle;
			var rootLink = $"http{(_httpContextAccessor.HttpContext.Request.IsHttps ? "s" : "")}://{_httpContextAccessor.HttpContext.Request.Host}/";
			var description = _config.BlogDescription;
			var xml = new XDocument(
				new XElement("rss",
					new XAttribute("version", "2.0"),
					new XAttribute(XNamespace.Xmlns + "atom", _atom),
					new XAttribute(XNamespace.Xmlns + "itunes", _itunes),
					new XAttribute(XNamespace.Xmlns + "content", "http://purl.org/rss/1.0/modules/content/"),
					new XElement("channel",
						new XElement("title", title),
						new XElement("link", rootLink),
						new XElement(_atom + "link",
							new XAttribute("href", rootLink + "home/rss"),
							new XAttribute("rel", "self"),
							new XAttribute("type", "application/rss+xml")),
						new XElement("description", description),
						new XElement("ttl", "15"),
						new XElement("language", _config.Language)
					)
				)
			);
			ModifyChannel(xml, rootLink);
			await AddItems(xml, rootLink);
			return xml;
		}

		private void ModifyChannel(XDocument xml, string rootLink)
		{
			var channel = xml.Element("rss").Element("channel");
			if (!string.IsNullOrEmpty(_config.FeedImageUrl))
			{
				channel.Add(new XElement(_itunes + "image", _config.FeedImageUrl));
				channel.Add(new XElement("image",
					new XElement("url", _config.FeedImageUrl),
					new XElement("title", _config.BlogTitle),
					new XElement("link", rootLink)));
			}

			if (!string.IsNullOrEmpty(_config.ItunesCategory))
			{
				channel.Add(new XElement(_itunes + "category", new XAttribute("text", _config.ItunesCategory)));
				channel.Add(new XElement(_itunes + "explicit", _config.ItunesExplicit));
			}

			if (!string.IsNullOrEmpty(_config.Author))
			{
				channel.Add(new XElement(_itunes + "author", _config.Author));
				channel.Add(new XElement("author", _config.Author));
			}

			if (!string.IsNullOrEmpty(_config.OwnerEmail) && !string.IsNullOrEmpty(_config.OwnerName))
			{
				channel.Add(new XElement(_itunes + "owner", 
					new XElement(_itunes + "name", _config.OwnerName),
					new XElement(_itunes + "email", _config.OwnerEmail)));
			}
		}
		private async Task AddItems(XDocument xml, string rootLink)
		{
			var posts = await _postRepository.GetLast20LiveAndPublic();
			var channel = xml.Element("rss").Element("channel");
			foreach (var item in posts)
				channel.Add(PopulateItem(item, rootLink));
		}

		private XElement PopulateItem(Post item, string rootLink)
		{
			var link = $"{rootLink}blog/{item.UrlTitle}";
			var element = new XElement("item",
				new XElement("title", item.Title),
				new XElement("author", item.Name),
				new XElement("link", link),
				new XElement("guid", link),
				new XElement("pubDate", item.TimeStamp.ToString("R")),
				new XElement("description", new XCData(item.FullText))
			);
			if (item.IsPodcastPost)
			{
				element.Add(new XElement("enclosure",
					new XAttribute("url", $"{rootLink}post/download/{item.PostID}"),
					new XAttribute("length", item.Size),
					new XAttribute("type", "audio/mpeg")));
				element.Add(new XElement(_itunes + "duration", item.Length));
				element.Add(new XElement(_itunes + "category", new XAttribute("text", _config.ItunesCategory)));
			}
			return element;
		}
	}
}