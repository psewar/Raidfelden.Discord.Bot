using Raidfelden.Entities;

namespace Raidfelden.Data.Pokemon
{
	public class UserRepository : GenericCastRepository<User, int, IUser>, IUserRepository
	{
		public UserRepository(raidfeldenContext context) : base(context)
		{
		}
	}
}
