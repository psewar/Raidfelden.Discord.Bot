﻿using Discord.Commands;
using Raidfelden.Discord.Bot.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NodaTime;
using System.Linq;
using System.Text;
using Raidfelden.Services;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Configuration;
using Raidfelden.Services.Extensions;

namespace Raidfelden.Discord.Bot.Modules
{
    [Group("raids")]
    public class RaidModule : BaseModule<SocketCommandContext, RaidChannel>
    {
        protected IRaidService RaidService { get; }
	    protected IOcrService OcrService;

        public RaidModule(IRaidService raidService, IEmojiService emojiService, IConfigurationService configurationService, IOcrService ocrService, ILocalizationService localizationService)
            : base(configurationService, emojiService, localizationService)
        {
            RaidService = raidService;
	        OcrService = ocrService;
        }

	    [Command("ocr")]
	    public async Task OcrAsync()
	    {
		    try
		    {
				var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
				// Subtract around 30 seconds to account for the delay to take and send a screenshot
			    utcNow = utcNow.Minus(Duration.FromSeconds(30));
				using (var httpClient = new HttpClient())
			    {
				    foreach (var attachment in Context.Message.Attachments)
				    {
					    if (!await IsImageUrlAsync(httpClient, attachment.Url)) continue;
					    var tempImageFile = string.Empty;
					    try
					    {
						    tempImageFile = Path.GetTempFileName() + "." + attachment.Url.Split('.').Last();
						    await DownloadAsync(httpClient, new Uri(attachment.Url), tempImageFile);
						    var response = OcrService.AddRaidAsync(typeof(i18n), utcNow, ChannelTimeZone, tempImageFile, InteractiveReactionLimit, Fences, false);
						    await ReplyWithInteractive(() => response, LocalizationService.Get(typeof(i18n), "Raids_Messages_Ocr_Successful_Title"));
					    }
					    finally
					    {
						    try
						    {
							    if (File.Exists(tempImageFile))
							    {
								    File.Delete(tempImageFile);
							    }
						    }
						    catch (Exception)
						    {
							    // Ignore
						    }
					    }
				    }
			    }
		    }
		    catch (Exception ex)
		    {
			    var innerstEx = ex.GetInnermostException();
			    Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
			}
	    }

	    private async Task<bool> IsImageUrlAsync(HttpClient httpClient, string url)
        {
	        using (var request = new HttpRequestMessage(HttpMethod.Head, url))
	        {
		        var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.Headers.ContentType.MediaType.StartsWith("image/");
                }
				return false;
	        }
		}

        private static async Task DownloadAsync(HttpClient httpClient, Uri requestUri, string filename)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                using (
                    Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
        }

        [Command("add"), Summary("Erlaubt es manuell Raids zu erfassen, die dann über das Raidsystem laufen.")]
        public async Task AddRaidAsync([Summary("Der Name der Arena.")]string gymName, [Summary("Der Name des Pokemon oder das Level des Raids")]string pokemonNameOrRaidLevel, [Summary("Die Zeit bis der Raid startet bzw. endet.")]string timeLeft)
        {
            try
            {
                // Only listen to commands in the configured channels or to exceptions
                if (!CanProcessRequest)
                {
                    return;
                }

				//Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
				var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
				var response = RaidService.AddAsync(typeof(i18n), utcNow, ChannelTimeZone, gymName, pokemonNameOrRaidLevel, timeLeft, InteractiveReactionLimit, Fences);
                await ReplyWithInteractive(() => response, LocalizationService.Get(typeof(i18n), "Raids_Messages_Successful_Title"));
            }
            catch (Exception ex)
            {
                var innerstEx = ex.GetInnermostException();
				Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
            }
        }

        [Command("hatch")]
        public async Task HatchRaidAsync(string gymName, string pokemonName)
        {
            try
            {
                // Only listen to commands in the configured channels or to exceptions
                if (!CanProcessRequest)
                {
                    return;
                }

				var response = RaidService.HatchAsync(typeof(i18n), gymName, pokemonName, InteractiveReactionLimit, Fences);
                await ReplyWithInteractive(() => response, LocalizationService.Get(typeof(i18n), "Raids_Messages_Successful_Title"));
            }
            catch (Exception ex)
            {
                var innerstEx = ex.GetInnermostException();
				Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
			}
        }

		[Command("list")]
		public async Task ListRaidAsync(string pokemonNameOrRaidLevel)
		{
			try
			{
				// Only listen to commands in the configured channels or to exceptions
				if (!CanProcessRequest)
				{
					return;
				}

				var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
				var response = RaidService.GetRaidList(typeof(i18n), utcNow, ChannelTimeZone, pokemonNameOrRaidLevel, Fences, "time", InteractiveReactionLimit, FormatRaidList);
				await ReplyWithInteractive(() => response, LocalizationService.Get(typeof(i18n), "Raids_Messages_Successful_Title"));
			}
			catch (Exception ex)
			{
				var innerstEx = ex.GetInnermostException();
				Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
			}
		}

	    private string FormatRaidList(RaidListInfo raidListInfo)
	    {
		    var builder = new StringBuilder();
		    foreach (var raid in raidListInfo.Raids)
		    {
			    builder.AppendLine(string.Concat(raid.Gym.Name, " ", raid.PokemonId ?? raid.Level, " ", raid.TimeSpawn));
		    }
		    return builder.ToString();
	    }
	}
}
