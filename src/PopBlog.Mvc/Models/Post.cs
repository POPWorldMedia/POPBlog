using System;

namespace PopBlog.Mvc.Models
{
	public class Post
	{
		public int PostID { get; set; }
		public int UserID { get; set; }
		public string Title { get; set; }
		public string FullText { get; set; }
		public DateTime TimeStamp { get; set; }
		public int Replies { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsClosed { get; set; }
		public bool IsLive { get; set; }
		public string UrlTitle { get; set; }
		public string Name { get; set; }
		public bool IsPodcastPost { get; set; }
		public string FileName { get; set; }
		public int DownloadCount { get; set; }
		public string Length { get; set; }
		public string Size { get; set; }
	}
}