using System.Collections.Generic;
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
		Task<User> Get(int userID);
		Task<User> GetUserByEmail(string email);
		Task<User> GetUserByName(string name);
		Task<IEnumerable<User>> GetAll();
		Task Delete(int userID);
		Task Update(int userID, string name, string email, string password);
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

		public async Task<User> Get(int userID)
		{
			return await _userRepository.Get(userID);
		}

		public async Task<User> GetUserByEmail(string email)
		{
			return await _userRepository.GetUserByEmail(email);
		}

		public async Task<User> GetUserByName(string name)
		{
			return await _userRepository.GetUserByName(name);
		}

		public async Task<IEnumerable<User>> GetAll()
		{
			return await _userRepository.GetAll();
		}

		public async Task Delete(int userID)
		{
			await _userRepository.Delete(userID);
		}

		public async Task Update(int userID, string name, string email, string password)
		{
			if (!string.IsNullOrEmpty(password))
			{
				password = password.GetSHA256Hash();
				await _userRepository.UpdatePassword(userID, password);
			}
			await _userRepository.Update(userID, name, email);
		}
	}
}