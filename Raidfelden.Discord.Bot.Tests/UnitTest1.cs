using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Discord.Bot.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Configuration.Providers.Fences.Novabot;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Services;
using Tesseract;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Tests
{
    [TestClass]
    public class UnitTest1
    {
		public IConfiguration Configuration { get; set; }

		protected IConfigurationService ConfigurationService { get; set; }

		public AppConfiguration Config { get; set; }

	    public string ConnectionString { get; set; }

	    public DbContextOptions ContextOptions { get; set; }

	    public UnitTest1()
	    {
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
	    }

        [TestMethod]
        public void TestMethod1()
        {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				
				var gymService = new GymService();
				var pokemonService = new PokemonService(new RaidbossService());
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
					var text = GetOcrResult(ocrService, basePath + "Screenshot_20180519-172950.png", engine);
					Assert.AreEqual(".raids add \"Feldschlösschen\" \"Kokowei\" 44:30", text, true);

					text = GetOcrResult(ocrService, basePath + "Schweizerkreuz_am_Bahnhof.png", engine);
					Assert.AreEqual(".raids add \"Schweizerkreuz am Bahnhof\" \"Flunkifer\" 41:29", text, true);

					text = GetOcrResult(ocrService, basePath + "RaidLevel5.png", engine);
					Assert.AreEqual(".raids add \"Water-Pacman at MFO Park\" \"5\" 46:15", text, true);

					text = GetOcrResult(ocrService, basePath + "RaidLevel4.png", engine);
					Assert.AreEqual(".raids add \"Bahnhof Graffiti Seebach\" \"4\" 48:34", text, true);

					text = GetOcrResult(ocrService, basePath + "A la Loko - Kyogre.png", engine);
					Assert.AreEqual(".raids add \"A la Loko\" \"Kyogre\" 8:7", text, true);
				}
			}
        }

		[TestMethod]
		public void PokemonNameCorrections()
	    {
			var gymService = new GymService();
			var pokemonService = new PokemonService(new RaidbossService());			
			using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
			    {
				    var basePath = @"Ressources\Pictures\Raids\";
					var text = GetOcrResult(ocrService, basePath + "Amonitas.jpg", engine);
					Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 20:5", text, true);

				    // Raidboss blocks part of the name
					text = GetOcrResult(ocrService, basePath + "Gundeldinger Krippe - Ho-Oh.png", engine);
					Assert.AreEqual(".raids add \"Gundeldinger Krippe\" \"Ho-Oh\" 2:43", text, true);

					// Raidboss blocks part of the name
					text = GetOcrResult(ocrService, basePath + "HoOzh.png", engine);
					Assert.AreEqual(".raids add \"Flying Bicycle\" \"Ho-Oh\" 23:26", text, true);

					text = GetOcrResult(ocrService, basePath + "Karpador.png", engine);
					Assert.AreEqual(".raids add \"Flying Bicycle\" \"Karpador\" 4:2", text, true);

					text = GetOcrResult(ocrService, basePath + "Absol-Theilsiefje.png", engine);
					Assert.AreEqual(".raids add \"Theilsiefje Säule\" \"Absol\" 44:24", text, true);

					// Raidboss blocks part of the name -- No way this is gonna work ^^
					//text = GetOcrResult(ocrService, basePath + "BHF Pratteln - Ho-Oh.png", engine);
					//Assert.AreEqual(".raids add \"BHF Pratteln\" \"Ho-Oh\" 44:36", text, true);
				}
			}
	    }

	    [TestMethod]
	    public void GymNameCorrections()
	    {
			var gymService = new GymService();
		    var pokemonService = new PokemonService(new RaidbossService());
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
			    {
				    var basePath = @"Ressources\Pictures\Raids\";
					var text = GetOcrResult(ocrService, basePath + "Jeckenbrunnen.png", engine);
					Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"5\" 21:44", text, true);

					text = GetOcrResult(ocrService, basePath + "Monument de Strasbourg.jpg", engine);
					Assert.AreEqual(".raids add \"Monument de Strasbourg (Elisabethenanlage)\" \"1\" 21:32", text, true);

					text = GetOcrResult(ocrService, basePath + "Schilderwald - Amonitas.png", engine);
					Assert.AreEqual(".raids add \"Schilderwald\" \"Amonitas\" 1:7", text, true);
				}
		    }
	    }

		[TestMethod]
		public void GalaxyS9WithMenuCorrection()
		{
			var gymService = new GymService();
			var pokemonService = new PokemonService(new RaidbossService());
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
                    var text = GetOcrResult(ocrService, basePath + "GalaxyS9WithMenu.jpg", engine);
                    Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 27:5", text, true);
                }
			}
		}

		[TestMethod]
		public void GalaxyS9PlusImageSize()
		{
			var gymService = new GymService();
			var pokemonService = new PokemonService(new RaidbossService());
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
					var text = GetOcrResult(ocrService, basePath + "ZurLandskrone-Karpador.jpg", engine);
					Assert.AreEqual(".raids add \"Zur Landskron\" \"Karpador\" 10:1", text, true);
				}
			}
		}

		[TestMethod]
		public void IPhoneXImageSize()
	    {
			var gymService = new GymService();
			var pokemonService = new PokemonService(new RaidbossService());
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = new OcrService(context, gymService, pokemonService);
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
					var text = GetOcrResult(ocrService, basePath + "IPhoneX-Ei.png", engine);
					Assert.AreEqual(".raids add \"The Gate\" \"5\" 28:42", text, true);

					text = GetOcrResult(ocrService, basePath + "IPhoneX-Boss.png", engine);
					Assert.AreEqual(".raids add \"The Gate\" \"Latios\" 39:35", text, true);
				}
			}
		}

	    private string GetOcrResult(OcrService ocrService, string filePath, TesseractEngine engine, FenceConfiguration[] fences = null)
	    {
		    var result = ocrService.AddRaidAsync(filePath, 4, fences).Result;
		    return result.Message;
	    }
    }
}

