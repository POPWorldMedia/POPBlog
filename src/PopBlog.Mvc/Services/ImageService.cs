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
	}
}