using System;
using Microsoft.Extensions.DependencyInjection;

namespace Raidfelden.Discord.Bot.Services
{
	public interface IServiceFactory
	{
		TService Build<TService>();
	}

    public class ServiceFactory : IServiceFactory
    {
	    private readonly IServiceProvider _serviceProvider;

	    public ServiceFactory(IServiceProvider serviceProvider)
	    {
		    _serviceProvider = serviceProvider;
	    }

		public TService Build<TService>() => _serviceProvider.GetService<TService>();
	}
}
