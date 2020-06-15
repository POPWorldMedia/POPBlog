using System.Threading.Tasks;
using PopBlog.Mvc.Extensions;
using PopBlog.Mvc.Models;
using PopBlog.Mvc.Repositories;

namespace PopBlog.Mvc.Services
{
	public interface IUserService
	{
		Task<bool> IsFirstUserCreated();
		Task Create(User user);
		Task<bool> IsValidUser(string email, string password);
		Task<User> GetUserByEmail(string email);
		Task<User> GetUserByName(string name);
	}

	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;

		public UserService(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<bool> IsFirstUserCreated()
		{
			return await _userRepository.IsFirstUserCreated();
		}

		public async Task Create(User user)
		{
			user.Password = user.Password.GetSHA256Hash();
			await _userRepository.Create(user);
		}

		public async Task<bool> IsValidUser(string email, string password)
		{
			var hashedPassword = password.GetSHA256Hash();
			var isValidUser = await _userRepository.IsValidUser(email, hashedPassword);
			return isValidUser;
		}

		public async Task<User> GetUserByEmail(string email)
		{
			return await _userRepository.GetUserByEmail(email);
		}

		public async Task<User> GetUserByName(string name)
		{
			return await _userRepository.GetUserByName(name);
		}
	}
}