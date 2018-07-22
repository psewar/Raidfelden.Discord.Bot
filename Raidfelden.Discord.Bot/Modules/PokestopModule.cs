using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Raidfelden.Configuration;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Discord.Bot.Services;
using Raidfelden.Services;
using Raidfelden.Services.Extensions;

namespace Raidfelden.Discord.Bot.Modules
{
	[Group("pokestops")]
    public class PokestopModule : BaseModule<SocketCommandContext, RaidChannel>
	{
		protected IPokestopService PokestopService { get; }
		
		public PokestopModule(IPokestopService pokestopService, IConfigurationService configurationService, IEmojiService emojiService, ILocalizationService localizationService) : base(configurationService, emojiService, localizationService)
		{
			PokestopService = pokestopService;
		}

		[Command("convert")]
		public async Task RegisterAsync(string name, string newName = null)
		{
			try
			{
				var response = await PokestopService.ConvertToGymAsync(name, newName, Fences);
				await ReplyWithInteractive(() => Task.FromResult(response as ServiceResponse), "Converted");
			}
			catch (Exception ex)
			{
				var innerstEx = ex.GetInnermostException();
				Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
			}
		}
	}
}
