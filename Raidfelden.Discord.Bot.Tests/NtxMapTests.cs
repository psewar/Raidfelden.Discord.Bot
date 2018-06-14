using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Configuration.Providers.Fences.Novabot;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Services;

namespace Raidfelden.Discord.Bot.Tests
{
	[TestClass]
	public class NtxMapTests
    {
		public IConfiguration Configuration { get; set; }

		protected IConfigurationService ConfigurationService { get; set; }

		public AppConfiguration Config { get; set; }

		public string ConnectionString { get; set; }

		public DbContextOptions ContextOptions { get; set; }

		private string basePath = @"Ressources\Pictures\Raids\Ntx\";

		public NtxMapTests()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
			Configuration = new ConfigurationBuilder()
						.AddNovabotGeoFencesFile("geofences.txt")
						.AddJsonFile("settings.json")
						.Build();

			var config = new AppConfiguration();
			Config = config;
			var section = Configuration.GetSection("AppConfiguration");
			section.Bind(config);
			ConnectionString = Configuration.GetConnectionString("ScannerDatabase");
			ContextOptions = new DbContextOptionsBuilder().UseMySql(ConnectionString).Options;
			ConfigurationService = new ConfigurationService(config, null);
		}

		[TestMethod]
		public void Ntx_BottomMenu1080X1920()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResult(ocrService, basePath + "1080x1920_BottomMenu_CumberlandPresbyterianChurch_Kyogre.png");
				Assert.AreEqual(".raids add \"Cumberland Presbyterian Church\" \"Kyogre\" 44:15", text, true);
			}
		}

		[TestMethod]
		public void Ntx_WithoutMenu750X1334()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResult(ocrService, basePath + "750x1334_SundownRanchLake_5.png");
				Assert.AreEqual(".raids add \"Sundown Ranch Lake\" \"5\" 44:15", text, true);
			}
		}
	}
}
