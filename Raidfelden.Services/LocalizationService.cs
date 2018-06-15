using System;
using System.Collections.Generic;
using System.Resources;
using System.Threading;

namespace Raidfelden.Services
{
	public interface ILocalizationService
	{
		string Get<T>(string resourceName, params object[] parameters);
		string Get(Type resourceType, string resourceName, params object[] parameters);
	}

	public class LocalizationService : ILocalizationService
	{
		protected static Dictionary<Type, ResourceManager> ResourceManagers { get; private set; }

		public LocalizationService()
		{
			ResourceManagers = new Dictionary<Type, ResourceManager>();
		}

		public string Get<T>(string resourceName, params object[] parameters)
		{
			return Get(typeof(T), resourceName, parameters);
		}

		public string Get(Type resourceType, string resourceName, params object[] parameters)
		{
			var manager = GetManager(resourceType);
			var resourceString = manager.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
			return string.Format(resourceString, parameters);
		}

		private ResourceManager GetManager(Type type)
		{
			if (!ResourceManagers.ContainsKey(type))
			{
				ResourceManagers.Add(type, new ResourceManager(type));
			}
			return ResourceManagers[type];
		}
    }
}
