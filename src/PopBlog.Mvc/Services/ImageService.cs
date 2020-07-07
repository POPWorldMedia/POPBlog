using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IImageService
	{
		Task<IEnumerable<ImageFolder>> GetAllImageFolders();
		Task CreateImageFolder(ImageFolder imageFolder);
		Task DeleteImageFolder(int imageFolderID);
		Task<IEnumerable<Image>> GetImagesByFolder(int? imageFolderID);
		Task<int> CreateImage(byte[] bytes, string fileName, string mimeType, int? imageFolderID);
		Task<Image> GetImage(int imageID);
		Task<byte[]> GetImageData(int imageID);
	}

	public class ImageService : IImageService
	{
		private readonly IImageFolderRepository _imageFolderRepository;
		private readonly IImageRepository _imageRepository;

		public ImageService(IImageFolderRepository imageFolderRepository, IImageRepository imageRepository)
		{
			_imageFolderRepository = imageFolderRepository;
			_imageRepository = imageRepository;
		}

		public async Task<IEnumerable<ImageFolder>> GetAllImageFolders()
		{
			return await _imageFolderRepository.GetAll();
		}

		public async Task CreateImageFolder(ImageFolder imageFolder)
		{
			await _imageFolderRepository.Create(imageFolder);
		}

		public async Task DeleteImageFolder(int imageFolderID)
		{
			await _imageRepository.RemoveImagesFromFolder(imageFolderID);
			await _imageFolderRepository.Delete(imageFolderID);
		}

		public async Task<IEnumerable<Image>> GetImagesByFolder(int? imageFolderID)
		{
			return await _imageRepository.GetImagesByFolder(imageFolderID);
		}

		public async Task<int> CreateImage(byte[] bytes, string fileName, string mimeType, int? imageFolderID)
		{
			var image = new Image
			{
				FileName = fileName,
				ImageFolderID = imageFolderID,
				MimeType = mimeType,
				TimeStamp = DateTime.UtcNow
			};
			return await _imageRepository.Create(bytes, image);
		}

		public async Task<Image> GetImage(int imageID)
		{
			return await _imageRepository.GetImage(imageID);
		}

		public async Task<byte[]> GetImageData(int imageID)
		{
			return await _imageRepository.GetImageData(imageID);
		}
	}
}