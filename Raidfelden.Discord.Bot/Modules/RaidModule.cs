using Discord.Commands;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions;
using Raidfelden.Discord.Bot.Services;
using Raidfelden.Discord.Bot.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Monocle;
using Tesseract;

namespace Raidfelden.Discord.Bot.Modules
{
    [Group("raids")]
    public class RaidModule : BaseModule<SocketCommandContext, RaidChannel>
    {
        protected IRaidService RaidService { get; }
	    private readonly Hydro74000Context _context;
	    private readonly IGymService _gymService;
	    private readonly IRaidbossService _raidbossService;
        private readonly IPokemonService _pokemonService;
        private readonly IEmojiService _emojiService;
        private readonly IConfigurationService _configurationService;

        public RaidModule(Hydro74000Context context, IRaidService raidService, IGymService gymService, IRaidbossService raidbossService, IPokemonService pokemonService, IEmojiService emojiService, IConfigurationService configurationService)
            : base(configurationService, emojiService)
        {
            RaidService = raidService;
	        _context = context;
	        _gymService = gymService;
	        _raidbossService = raidbossService;
            _pokemonService = pokemonService;
            _emojiService = emojiService;
            _configurationService = configurationService;
        }

        [Command("ocr")]
        public async Task OcrAsync()
        {
	        using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
	        {
		        foreach (var attachment in Context.Message.Attachments)
		        {
			        if (IsImageUrl(attachment.Url))
			        {
				        string tempImageFile = string.Empty;
				        try
				        {
					        tempImageFile = Path.GetTempFileName() + "." + attachment.Url.Split('.').Last();
					        await DownloadAsync(new Uri(attachment.Url), tempImageFile);
					        using (var image = Image.Load(tempImageFile))
					        {
						        using (var raidImage = new RaidImage<Rgba32>(image, _gymService, _pokemonService))
						        {

							        var gymName = raidImage.GetFragmentString(engine, ImageFragmentType.GymName, _context, Fences);
							        var timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.EggTimer, _context, Fences);
							        var isRaidboss = string.IsNullOrWhiteSpace(timerValue);
							        if (isRaidboss)
							        {
								        var pokemonName = raidImage.GetFragmentString(engine, ImageFragmentType.PokemonName, _context, Fences);
								        timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.RaidTimer, _context, Fences);
								        var timer = TimeSpan.Parse(timerValue);
								        await ReplySuccessAsync("Wär ich nicht ein Test, würde ich folgendes ausführen:",
									        $".raids add \"{gymName}\" \"{pokemonName}\" {string.Concat(timer.Minutes, ":", timer.Seconds)} ");
							        }
							        else
							        {
								        var eggLevel = raidImage.GetFragmentString(engine, ImageFragmentType.EggLevel, _context, Fences);
								        var timer = TimeSpan.Parse(timerValue);
								        await ReplySuccessAsync("Wär ich nicht ein Test, würde ich folgendes ausführen:",
									        $".raids add \"{gymName}\" \"{eggLevel}\" {string.Concat(timer.Minutes, ":", timer.Seconds)} ");
							        }
						        }
					        }
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
	        await ReplySuccessAsync("Ocr erfolgreich", "Yeah man!");
        }

        private bool IsImageUrl(string url)
        {
            return true;
        }

        private static async Task DownloadAsync(Uri requestUri, string filename)
        {
            using (var httpClient = new HttpClient())
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
