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
	}
}