using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace PopBlog.Mvc.Repositories
{
	public interface IStorageRepository
	{
		Task Upload(string fileName, Stream stream);
		Task Delete(string fileName);
		string GetUrl(string fileName);
	}

	public class StorageRepository : IStorageRepository
	{
		private readonly IConfig _config;

		public StorageRepository(IConfig config)
		{
			_config = config;
		}

		public async Task Upload(string fileName, Stream stream)
		{
			stream.Position = 0;
			var serviceClient = new BlobServiceClient(_config.StorageConnectionString);
			var containerClient = serviceClient.GetBlobContainerClient(_config.StorageContainerName);
			await containerClient.DeleteBlobIfExistsAsync(fileName);
			await containerClient.UploadBlobAsync(fileName, stream);
		}

		public async Task Delete(string fileName)
		{
			var serviceClient = new BlobServiceClient(_config.StorageConnectionString);
			var containerClient = serviceClient.GetBlobContainerClient(_config.StorageContainerName);
			await containerClient.DeleteBlobIfExistsAsync(fileName);
		}

		public string GetUrl(string fileName)
		{
			return $"{_config.StorageAccountBaseUrl}/{_config.StorageContainerName}/{Uri.EscapeDataString(fileName)}";
		}
	}
}