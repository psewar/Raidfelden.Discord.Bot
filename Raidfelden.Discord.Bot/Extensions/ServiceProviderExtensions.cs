using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Discord.Bot.Extensions
{
    public static class ServiceProviderExtensions
    {
		public static T Resolve<T>(this IServiceProvider serviceProvider) where T: class
		{
			var obj = serviceProvider.GetService(typeof(T));
			return obj as T;
		}
    }
}
