using Raidfelden.Data.Pokemon.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Pokemon
{
	public class UserRepository : GenericCastRepository<User, int, IUser>, IUserRepository
	{
		public UserRepository(RaidfeldenContext context) : base(context)
		{
		}
	}
}
