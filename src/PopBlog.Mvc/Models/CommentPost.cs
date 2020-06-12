namespace PopBlog.Mvc.Models
{
	public class CommentPost
	{
		public int PostID { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string WebSite { get; set; }
		public string FullText { get; set; }
	}
}