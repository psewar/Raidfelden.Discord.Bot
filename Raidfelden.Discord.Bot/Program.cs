using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raidfelden.Discord.Bot.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Raidfelden.Discord.Bot.Extensions;
using Raidfelden.Discord.Bot.Modules;
using Raidfelden.Discord.Bot.Services;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Discord.Bot.Configuration.Providers.Fences.Novabot;
using Discord.Addons.Interactive;
using Raidfelden.Discord.Bot.Monocle;
using Microsoft.EntityFrameworkCore;

namespace Raidfelden.Discord.Bot
{
    public class Program
    {
		private DiscordSocketClient _client;
		private CommandService _commands;

		public static IServiceProvider ServiceProvider { get; private set; }

        public static IConfiguration Configuration { get; set; }

        protected IConfigurationService ConfigurationService { get; set; }

        public static AppConfiguration Config { get; set; }

        static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                                .AddNovabotGeoFencesFile("geofences.txt")
                                .AddJsonFile("settings.json")
                                .Build();

            var config = new AppConfiguration();
            Config = config;
            var section = Configuration.GetSection("AppConfiguration");
            section.Bind(config);

            var cultureInfo = new CultureInfo(config.CultureCode);
            i18n.Culture = cultureInfo;
			new Program().RunBotAsync(config).GetAwaiter().GetResult();
        }

		public async Task RunBotAsync(AppConfiguration configuration)
		{
            _client = new DiscordSocketClient();
			_commands = new CommandService();

            var connectionString = Configuration.GetConnectionString("ScannerDatabase");
            var fencesSection = Configuration.GetSection("FencesConfiguration");
            var fences = new FencesConfiguration();
            fencesSection.Bind(fences);
            ConfigurationService = new ConfigurationService(configuration, fences);
            ServiceProvider = new ServiceCollection()
                //.AddLocalization(options => options.ResourcesPath = "Resources")
                .AddSingleton(configuration)
                .AddSingleton(fences)
                .AddEntityFrameworkMySql()
                .AddDbContext<Hydro74000Context>(options => options.UseMySql(connectionString))
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<IPokemonService, PokemonService>()
                .AddSingleton<IRaidbossService, RaidbossService>()
                .AddSingleton<IGymService, GymService>()
                .AddSingleton<IRaidService, RaidService>()
                .AddSingleton<IEmojiService, EmojiService>()
                .AddSingleton<IConfigurationService>(ConfigurationService)
                .AddSingleton<InteractiveService>()
                .AddScoped<IOcrService, OcrService>()
                .AddScoped<ITestModule, TestModule>()
				.AddScoped<ILocalizationService, LocalizationService>()
                .BuildServiceProvider();

            string botToken = configuration.BotToken;
            // Event Subscriptions
            _client.Log += Log;

            _client.Disconnected += _client_DisconnectedAsync;

			await RegisterCommandsAsync();

			await _client.LoginAsync(TokenType.Bot, botToken);

			await _client.StartAsync();

			await Task.Delay(-1);
		}

        private async Task _client_DisconnectedAsync(Exception arg)
        {
			// This is probably not needed on windows machine, but on my mac server I get frequent disconnectes from which the bot can't recover
			// So I stop the bot with an exit code and wait for the sh script to restart the bot
	        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
	        if (!isWindows)
	        {
		        Console.WriteLine("Got a disconnect, closing the Bot and wait for the script to restart it");
		        Environment.Exit(1);
	        }
        }

        private Task Log(LogMessage arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}

		public async Task RegisterCommandsAsync()
		{
			_client.MessageReceived += HandleCommandAsync;
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			var message = arg as SocketUserMessage;
			if (message == null || message.Author.IsBot) { return; }

			int argPos = 0;
            var context = new SocketCommandContext(_client, message);
            var prefix = ConfigurationService.GetCommandPrefix(context);
            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				
				var result = await _commands.ExecuteAsync(context, argPos, ServiceProvider);
				if (!result.IsSuccess)
				{
					Console.WriteLine(result.ErrorReason);
				}
			}

            var ocrChannels = ConfigurationService.GetChannelConfigurations(context).Where(e => e.IsOcrAllowed).Select(e => e.Name.ToLowerInvariant()).ToArray();
            if ((ocrChannels.Contains(message.Channel.Name.ToLowerInvariant()) || message.Author.Username == "psewar") && message.Attachments.Count > 0)
            {
                var result = await _commands.ExecuteAsync(context, "raids ocr", ServiceProvider);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
		}
	}
}
