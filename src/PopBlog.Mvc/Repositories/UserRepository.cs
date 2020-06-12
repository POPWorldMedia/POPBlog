using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using PopBlog.Mvc.Models;

namespace PopBlog.Mvc.Repositories
{
	public interface IUserRepository
	{
		Task<bool> IsFirstUserCreated();
		Task Create(User user);
	}

	public class UserRepository : IUserRepository
	{
		private readonly IConfig _config;

		public UserRepository(IConfig config)
		{
			_config = config;
		}

		public async Task<bool> IsFirstUserCreated()
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			var hasOneOrMoreUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users") > 0;
			return hasOneOrMoreUsers;
		}

		public async Task Create(User user)
		{
			await using var connection = new SqlConnection(_config.ConnectionString);
			await connection.ExecuteAsync("INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)", user);
		}
	}
}