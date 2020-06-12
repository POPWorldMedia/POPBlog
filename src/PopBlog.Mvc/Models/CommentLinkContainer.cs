using System;

namespace PopBlog.Mvc.Models
{
	public class CommentLinkContainer
	{
		public string Title { get; set; }
		public string UrlTitle { get; set; }
		public DateTime TimeStamp { get; set; }
		public int CommentID { get; set; }
		public int PostID { get; set; }
	}
}