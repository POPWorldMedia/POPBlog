using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace PopBlog.Mvc.Repositories
{
	public interface IIpBanRepository
	{
		Task<int> GetIpMatchCount(string ip);
		Task Add(string ip);
		Task Delete(string ip);
		Task<IEnumerable<string>> GetAll();
	}

	public class IpBanRepository : IIpBanRepository
	{
		private readonly IConfig _config;

		public IpBanRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<int> GetIpMatchCount(string ip)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) from IpBans WHERE CHARINDEX(IP, @IP, 0) > 0", new {IP = ip});
			return count;
		}

		public async Task Add(string ip)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO IpBans (IP) VALUES (@IP)", new {IP = ip});
		}

		public async Task Delete(string ip)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("DELETE FROM IpBans WHERE IP = @IP", new { IP = ip });
		}

		public async Task<IEnumerable<string>> GetAll()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var list = await connection.QueryAsync<string>("SELECT IP FROM IpBans ORDER BY IP");
			return list;
		}
	}
}