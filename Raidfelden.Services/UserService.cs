using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Data;
using Raidfelden.Entities;

namespace Raidfelden.Services
{
	public interface IUserService
	{
		Task<ServiceResponse<IUser>> RegisterAsync(string username, double latitude, double longitude, string friendshipCode, ulong? discordId = null, string discordMention = null);
		Task<ServiceResponse<IUser>> GetByIdAsync(int id);
		Task<ServiceResponse<IUser>> GetByUsernameAsync(string username);
		Task<ServiceResponse<IUser>> GetByDiscordIdAsync(ulong discordId);
		Task<ServiceResponse<IUser>> DeleteAsync(IUser user);
		Task<ServiceResponse<IUser>> UpdateLocationAsync(IUser user, double latitude, double longitude);
	}

    public class UserService : IUserService
	{
		protected IUserRepository UserRepository { get; }

		public UserService(IUserRepository userRepository)
		{
			UserRepository = userRepository;
		}
		public async Task<ServiceResponse<IUser>> RegisterAsync(string username, double latitude, double longitude, string friendshipCode, ulong? discordId = null, string discordMention = null)
		{
			var users = await UserRepository.FindByAsync(e => e.Name == username.ToLowerInvariant());
			if (users.Count > 0)
			{
				return new ServiceResponse<IUser>(false, "User does already exist", users.First());
			}

			var user = UserRepository.CreateInstance();
			user.Name = username.ToLowerInvariant();
			user.Latitude = latitude;
			user.Longitude = longitude;
			user.FriendshipCode = friendshipCode;
			user.DiscordId = discordId;
			user.DiscordMention = discordMention;
			await UserRepository.AddAsync(user);
			return new ServiceResponse<IUser>(true, "User registered", user);
		}

		public async Task<ServiceResponse<IUser>> GetByIdAsync(int id)
		{
			var user = await UserRepository.GetAsync(id);
			return user == null ? await CreateUserDoesNotExistResponse() 
								: await CreateUserFoundResponse(user);
		}

		public async Task<ServiceResponse<IUser>> GetByUsernameAsync(string username)
		{
			var user = await UserRepository.FindAsync(e => e.Name == username.ToLowerInvariant());
			return user == null ? await CreateUserDoesNotExistResponse()
								: await CreateUserFoundResponse(user);
		}

		public async Task<ServiceResponse<IUser>> GetByDiscordIdAsync(ulong discordId)
		{
			var user = await UserRepository.FindAsync(e => e.DiscordId == discordId);
			return user == null ? await CreateUserDoesNotExistResponse()
								: await CreateUserFoundResponse(user);
		}

		public async Task<ServiceResponse<IUser>> DeleteAsync(IUser user)
		{
			var userInRepository = await UserRepository.GetAsync(user.Id);
			userInRepository.Active = false;
			await UserRepository.SaveAsync();
			return new ServiceResponse<IUser>(true, "User deactivated", user);
		}

		public async Task<ServiceResponse<IUser>> UpdateLocationAsync(IUser user, double latitude, double longitude)
		{
			var userInRepository = await UserRepository.GetAsync(user.Id);
			userInRepository.Latitude = latitude;
			userInRepository.Longitude = longitude;
			await UserRepository.UpdateAsync(user.Id, userInRepository);
			return new ServiceResponse<IUser>(true, "Location updated.", userInRepository);
		}

		private async Task<ServiceResponse<IUser>> CreateUserDoesNotExistResponse()
		{
			return await Task.FromResult(new ServiceResponse<IUser>(false, "User does not exist", null));
		}

		private async Task<ServiceResponse<IUser>> CreateUserFoundResponse(IUser user)
		{
			return await Task.FromResult(new ServiceResponse<IUser>(true, "User found", user));
		}
	}
}
