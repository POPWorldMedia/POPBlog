using System;

namespace PopBlog.Mvc.Models
{
	public class CommentLink
	{
		public int CommentID { get; set; }
		public string UrlTitle { get; set; }
		public string Title { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}