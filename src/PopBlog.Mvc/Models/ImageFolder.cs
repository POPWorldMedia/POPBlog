namespace PopBlog.Mvc.Models
{
	public class ImageFolder
	{
		public int ImageFolderID { get; set; }
		public string Name { get; set; }
		public int? ParentImageFolderID { get; set; }
	}
}