using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Raidfelden.Data.Pokemon
{
    public static class Setup
    {
		public static IServiceCollection ConfigurePokemon(this IServiceCollection services, string connectionString)
        {
            //services.AddDbContext<RaidfeldenContext>(options => options.UseMySql(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
			return services;
		}
	}
}
