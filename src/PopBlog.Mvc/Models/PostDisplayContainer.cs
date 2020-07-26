using System.Collections.Generic;

namespace PopBlog.Mvc.Models
{
	public class PostDisplayContainer
	{
		public Post Post { get; set; }
		public IEnumerable<Comment> Comments { get; set; }
	}
}