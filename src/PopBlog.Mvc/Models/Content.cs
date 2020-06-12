using System;

namespace PopBlog.Mvc.Models
{
	public class Content
	{
		public string ContentID { get; set; }
		public string Title { get; set; }
		public DateTime LastUpdated { get; set; }
		public string FullText { get; set; }
	}
}