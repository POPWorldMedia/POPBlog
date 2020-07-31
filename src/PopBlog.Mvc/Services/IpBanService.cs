using System.Collections.Generic;
using System.Threading.Tasks;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IIpBanService
	{
		Task Add(string ip);
		Task Delete(string ip);
		Task<IEnumerable<string>> GetAll();
		Task<bool> IsIpBanned(string ip);
	}

	public class IpBanService : IIpBanService
	{
		private readonly IIpBanRepository _ipBanRepository;

		public IpBanService(IIpBanRepository ipBanRepository)
		{
			_ipBanRepository = ipBanRepository;
		}

		public async Task Add(string ip)
		{
			await _ipBanRepository.Add(ip.Trim());
		}

		public async Task Delete(string ip)
		{
			await _ipBanRepository.Delete(ip);
		}

		public async Task<IEnumerable<string>> GetAll()
		{
			return await _ipBanRepository.GetAll();
		}

		public async Task<bool> IsIpBanned(string ip)
		{
			var count = await _ipBanRepository.GetIpMatchCount(ip);
			return count > 0;
		}
	}
}