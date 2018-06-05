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
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
					var text = GetRaidText(basePath + "Screenshot_20180519-172950.png", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Feldschlösschen\" \"Kokowei\" 44:30", text, true);

					text = GetRaidText(basePath + "Schweizerkreuz_am_Bahnhof.png", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Schweizerkreuz am Bahnhof\" \"Flunkifer\" 41:29", text, true);

					text = GetRaidText(basePath + "RaidLevel5.png", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Water-Pacman at MFO Park\" \"5\" 46:15", text, true);

					text = GetRaidText(basePath + "RaidLevel4.png", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Bahnhof Graffiti Seebach\" \"4\" 48:34", text, true);

					text = GetRaidText(basePath + "A la Loko - Kyogre.png", engine, gymService, pokemonService, context);
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
			    using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
			    {
				    var basePath = @"Ressources\Pictures\Raids\";
				    var text = GetRaidText(basePath + "Amonitas.jpg", engine, gymService, pokemonService, context);
				    Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 20:5", text, true);

				    // Raidboss blocks part of the name
				    text = GetRaidText(basePath + "Gundeldinger Krippe - Ho-Oh.png", engine, gymService, pokemonService, context);
				    Assert.AreEqual(".raids add \"Gundeldinger Krippe\" \"Ho-Oh\" 2:43", text, true);

				    // Raidboss blocks part of the name
				    text = GetRaidText(basePath + "HoOzh.png", engine, gymService, pokemonService, context);
				    Assert.AreEqual(".raids add \"Flying Bicycle\" \"Ho-Oh\" 23:26", text, true);

				    text = GetRaidText(basePath + "Karpador.png", engine, gymService, pokemonService, context);
				    Assert.AreEqual(".raids add \"Flying Bicycle\" \"Karpador\" 4:2", text, true);

					text = GetRaidText(basePath + "Absol-Theilsiefje.png", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Theilsiefje Säule\" \"Absol\" 44:24", text, true);
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
			    using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
			    {
				    var basePath = @"Ressources\Pictures\Raids\";
				    var text = GetRaidText(basePath + "Jeckenbrunnen.png", engine, gymService, pokemonService, context);
				    Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"5\" 21:44", text, true);

					text = GetRaidText(basePath + "Monument de Strasbourg.jpg", engine, gymService, pokemonService, context);
					Assert.AreEqual(".raids add \"Monument de Strasbourg (Elisabethenanlage)\" \"1\" 21:32", text, true);
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
				using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
				{
					var basePath = @"Ressources\Pictures\Raids\";
                    var text = GetRaidText(basePath + "GalaxyS9WithMenu.jpg", engine, gymService, pokemonService, context);
                    Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 27:5", text, true);
                }
			}
		}

		private string GetRaidText(string filePath, TesseractEngine engine, IGymService gymService, IPokemonService pokemonService, Hydro74000Context context)
        {
            using (var image = Image.Load(filePath))
            {
                using (var raidImage = new RaidImage<Rgba32>(image, gymService, pokemonService))
                {
                    var gymName = raidImage.GetFragmentString(engine, ImageFragmentType.GymName, context);
                    var timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.EggTimer, context);
                    var isRaidboss = !TimeSpan.TryParse(timerValue, out TimeSpan timer);
                    if (isRaidboss)
                    {
                        var pokemonName = raidImage.GetFragmentString(engine, ImageFragmentType.PokemonName, context);
                        timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.RaidTimer, context);
                        timer = TimeSpan.Parse(timerValue);
                        return $".raids add \"{gymName}\" \"{pokemonName}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
                    }
                    else
                    {
                        var eggLevel = raidImage.GetFragmentString(engine, ImageFragmentType.EggLevel, context);
                        //var timer = TimeSpan.Parse(timerValue);
                        return $".raids add \"{gymName}\" \"{eggLevel}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
                    }
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
                using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
                {
                    var basePath = @"Ressources\Pictures\Raids\";
                    var text = GetRaidText(basePath + "ZurLandskrone-Karpador.jpg", engine, gymService, pokemonService, context);
                    Assert.AreEqual(".raids add \"Zur Landskron\" \"Karpador\" 10:1", text, true);
                }
            }
        }
    }
}

