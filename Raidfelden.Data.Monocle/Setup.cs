﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Data.Monocle
{
    public static class Setup
    {
		public static IServiceCollection ConfigureMonocle(this IServiceCollection services)
		{
			//services.AddDbContext<Hydro74000Context>();
			services.AddScoped<IGymRepository, GymRepository>();
			//services.AddScoped<IAccountRepository, AccountRepository>();
			//services.AddScoped<ICommonRepository, CommonRepository>();
			//services.AddScoped<IFortRepository, FortRepository>();
			//services.AddScoped<IFortSightingRepository, FortSightingRepository>();
			//services.AddScoped<IGymDefenderRepository, GymDefenderRepository>();
			//services.AddScoped<IMysterySightingRepository, MysterySightingRepository>();
			//services.AddScoped<IParkRepository, ParkRepository>();
			//services.AddScoped<IPokestopRepository, PokestopRepository>();
			//services.AddScoped<IRaidRepository, RaidRepository>();
			//services.AddScoped<ISightingRepository, SightingRepository>();
			//services.AddScoped<ISpawnpointRepository, SpawnpointRepository>();
			//services.AddScoped<IWeatherRepository, WeatherRepository>();
			return services;
		}
	}
}