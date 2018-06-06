using System.Resources;
using System.Threading;
using Raidfelden.Discord.Bot.Resources;

namespace Raidfelden.Discord.Bot.Services
{
	public interface ILocalizationService
	{
		string Get(string resourceName, params object[] parameters);
	}

	public class LocalizationService : ILocalizationService
    {
		public LocalizationService()
		{
			ResourceManager = new ResourceManager(typeof(i18n));
		}

		protected ResourceManager ResourceManager { get; }

		public string Get(string resourceName, params object[] parameters)
		{
			var resourceString = ResourceManager.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
			return string.Format(resourceString, parameters);
		}
    }
}
