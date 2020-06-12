using System;

namespace PopBlog.Mvc.Models
{
	public class Comment
	{
		public int CommentID { get; set; }
		public int PostID { get; set; }
		public int? UserID { get; set; }
		public string FullText { get; set; }
		public DateTime TimeStamp { get; set; }
		public bool IsLive { get; set; }
		public string IP { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string WebSite { get; set; }
	}
}