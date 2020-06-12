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
	}
}