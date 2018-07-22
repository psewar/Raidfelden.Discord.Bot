using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Raidfelden.Data.Monocle
{
    public static class Setup
    {
		public static IServiceCollection ConfigureMonocle(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<Hydro74000Context>(options => options.UseMySql(connectionString));
            services.AddScoped<IGymRepository, GymRepository>();
            services.AddScoped<IRaidRepository, RaidRepository>();
	        services.AddScoped<IPokestopRepository, PokestopRepository>();
			return services;
		}
	}
}
