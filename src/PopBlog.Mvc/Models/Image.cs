using System;

namespace PopBlog.Mvc.Models
{
	public class Image
	{
		public int ImageID { get; set; }
		public string MimeType { get; set; }
		public string FileName { get; set; }
		public int? ImageFolderID { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}