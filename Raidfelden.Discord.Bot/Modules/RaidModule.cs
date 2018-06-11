using Discord.Commands;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions;
using Raidfelden.Discord.Bot.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Modules
{
    [Group("raids")]
    public class RaidModule : BaseModule<SocketCommandContext, RaidChannel>
    {
        protected IRaidService RaidService { get; }
		protected ILocalizationService LocalizationService { get; }
	    protected IOcrService OcrService;

        public RaidModule(IRaidService raidService, IEmojiService emojiService, IConfigurationService configurationService, IOcrService ocrService, ILocalizationService localizationService)
            : base(configurationService, emojiService)
        {
            RaidService = raidService;
	        LocalizationService = localizationService;
	        OcrService = ocrService;
        }

	    [Command("ocr")]
	    public async Task OcrAsync()
	    {
		    try
		    {
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
						    var response = OcrService.AddRaidAsync(tempImageFile, 4, Fences, false);
						    await ReplyWithInteractive(() => response, "OCR-Erkennung erfolgreich");
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
			    await ReplyFailureAsync($"Ein unerwarteter Fehler ist aufgetreten: {innerstEx.Message}");
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
				var response = RaidService.AddAsync(gymName, pokemonNameOrRaidLevel, timeLeft, 4, Fences);
                await ReplyWithInteractive(() => response, "Raid erfolgreich eingetragen");
            }
            catch (Exception ex)
            {
                var innerstEx = ex.GetInnermostException();
                await ReplyFailureAsync($"Ein unerwarteter Fehler ist aufgetreten: {innerstEx.Message}");
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

				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");				
				var response = RaidService.HatchAsync(gymName, pokemonName, 4, Fences);
                await ReplyWithInteractive(() => response, "Raid erfolgreich erweitert");
            }
            catch (Exception ex)
            {
                var innerstEx = ex.GetInnermostException();
                await ReplyFailureAsync($"Ein unerwarteter Fehler ist aufgetreten: {innerstEx.Message}");
            }
        }
	}
}
