using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Raidfelden.Services;
using Raidfelden.Configuration;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests
{
    [TestClass]
    public class UnitTest1
    {
		protected IConfigurationService ConfigurationService { get; set; }

		public AppConfiguration Config { get; set; }

	    public string ConnectionString { get; set; }

	    public DbContextOptions ContextOptions { get; set; }

        private string basePath = @"Ressources\Pictures\Raids\";

        public UnitTest1()
	    {
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
		    ConfigurationService = new ConfigurationService();
            ConnectionString = ConfigurationService.GetConnectionString("ScannerDatabase");
            ContextOptions = new DbContextOptionsBuilder().UseMySql(ConnectionString).Options;
        }

        [TestMethod]
        public void TestMethod1()
        {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Screenshot_20180519-172950.png");
				Assert.AreEqual(".raids add \"Feldschlösschen\" \"Kokowei\" 44:30", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Schweizerkreuz_am_Bahnhof.png");
				Assert.AreEqual(".raids add \"Schweizerkreuz am Bahnhof\" \"Flunkifer\" 41:29", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "A la Loko - Kyogre.png");
				Assert.AreEqual(".raids add \"A la Loko\" \"Kyogre\" 8:7", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RestzeitNichtAuswertbar.jpg");
                Assert.AreEqual(".raids add \"COOP in Basel\" \"5\" 56:57", text, true);
            }
        }

		[TestMethod]
		public void PokemonNameCorrections()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Amonitas.jpg");
				Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 20:5", text, true);

				// Raidboss blocks part of the name
				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Gundeldinger Krippe - Ho-Oh.png");
				Assert.AreEqual(".raids add \"Gundeldinger Krippe\" \"Ho-Oh\" 2:43", text, true);

				// Raidboss blocks part of the name
				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "HoOzh.png");
				Assert.AreEqual(".raids add \"Flying Bicycle\" \"Ho-Oh\" 23:26", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Karpador.png");
				Assert.AreEqual(".raids add \"Flying Bicycle\" \"Karpador\" 4:2", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Absol-Theilsiefje.png");
				Assert.AreEqual(".raids add \"Theilsiefje Säule\" \"Absol\" 44:24", text, true);

				// Raidboss blocks part of the name - Could be possible to fix
				//text = GetOcrResult(ocrService, basePath + "HoOhAsAmonitas.png", engine);
				//Assert.AreEqual(".raids add \"Steinmann am Rhein\" \"Ho-Oh\" 25:16", text, true);

				// Raidboss blocks part of the name -- No way this is gonna work ^^
				//text = GetOcrResult(ocrService, basePath + "BHF Pratteln - Ho-Oh.png", engine);
				//Assert.AreEqual(".raids add \"BHF Pratteln\" \"Ho-Oh\" 44:36", text, true);
			}
	    }

	    [TestMethod]
	    public void GymNameCorrections()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Jeckenbrunnen.png");
				Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"5\" 21:44", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Monument de Strasbourg.jpg");
				Assert.AreEqual(".raids add \"Monument de Strasbourg (Elisabethenanlage)\" \"1\" 21:32", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Schilderwald - Amonitas.png");
				Assert.AreEqual(".raids add \"Schilderwald\" \"Amonitas\" 1:7", text, true);
		    }
	    }

		[TestMethod]
		public void IPhoneXImageSize()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "IPhoneX-Ei.png");
				Assert.AreEqual(".raids add \"The Gate\" \"5\" 28:42", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "IPhoneX-Boss.png");
				Assert.AreEqual(".raids add \"The Gate\" \"Latios\" 39:35", text, true);
			}
		}

		[TestMethod]
		public void BottomMenuBar1080x1920Correction()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BottomMenuBar-Boss-1080x1920.jpg");
				Assert.AreEqual(".raids add \"St. Johanns-Tor\" \"Walraisa\" 23:36", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BottomMenuBar-Egg5-1080x1920.jpg");
				Assert.AreEqual(".raids add \"Spalentor\" \"5\" 28:28", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BottomMenuBar-Egg4-1080x1920.jpg");
				Assert.AreEqual(".raids add \"Organspender Gedenkstein\" \"4\" 36:21", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BossWithMenu1080X1920.png");
				Assert.AreEqual(".raids add \"International Imaginary Museum\" \"Kokowei\" 34:55", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "GymNameNotRecognized1080X1920.jpg");
				Assert.AreEqual(".raids add \"Kleinbasler Basiliskenbrünnchen\" \"5\" 37:51", text, true);
			}
		}

	    [TestMethod]
	    public void WithoutMenuBar1080x1920Correction()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				//TODO: Get a better picture with visible raid timer
				//var text = GetOcrResult(ocrService, basePath + "Boss1080X2220.jpg", engine);
				//Assert.AreEqual(".raids add \"The Gate\" \"Sniebel\" ??:31", text, true);

				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Egg1080x2220.jpg");
				Assert.AreEqual(".raids add \"Moderne Konferenz\" \"5\" 55:9", text, true);
			}
		}

		[TestMethod]
		public void BottomMenu1080X2220()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "ZurLandskrone-Karpador.jpg");
				Assert.AreEqual(".raids add \"Zur Landskron\" \"Karpador\" 10:1", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Egg1080X2220Herwig.jpg");
				Assert.AreEqual(".raids add \"Skulptur Zwei Schwangere Frauen \" \"4\" 56:54", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "GalaxyS9WithMenu2.jpg");
				Assert.AreEqual(".raids add \"Rheinfelden Bahnhof\" \"5\" 48:59", text, true);
			}
		}

		[TestMethod]
		public void BothMenu1080X2220()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);			
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "HoOh1080X2220.jpg");
				Assert.AreEqual(".raids add \"Warmbacher Kreuz\" \"Ho-Oh\" 25:41", text, true);

				text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BothMenu1080X2220Egg.jpg");
				Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"1\" 51:3", text, true);
			}
		}

		[TestMethod]
		public void BottomMenu1440X2960()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "GalaxyS9WithMenu.jpg");
				Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 27:5", text, true);
			}
		}

		[TestMethod]
		public void BottomMenu900X1600()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "Nexus900x1600Boss.png");
				Assert.AreEqual(".raids add \"Helix Fountain\" \"Snorunt\" 41:40", text, true);
			}
		}

		[TestMethod]
	    public void NullRefException()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				// The problem here was a to strict BinaryTreshold where the bot would not recognize the gym name anymore
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "NullRefException1.jpg");
				Assert.AreEqual(".raids add \"Die Venus in den Büschen\" \"4\" 49:48", text, true);
		    }
	    }

		[TestMethod]
		public void Interactive()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				// Get the interactive Response
				var interactiveResult = OcrTestsHelper.GetOcrResult(ocrService, basePath + "Kreuz - Kyogre (Interactive).png");
				// The user clicked on the second entry
				var result = interactiveResult.InterActiveCallbacks.Skip(2).First().Value();
				// Raid successfully added
				var text = result.Result.Message;
				Assert.AreEqual(".raids add \"Kreuz\" \"Kyogre\" 8:7", text, true);
			}
		}

		[TestMethod]
        public void EggLevels()
        {
            using (var context = new Hydro74000Context(ContextOptions))
            {
                var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
                var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RaidLevel5.png");
                Assert.AreEqual(".raids add \"Water-Pacman at MFO Park\" \"5\" 46:15", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RaidLevel4.png");
                Assert.AreEqual(".raids add \"Bahnhof Graffiti Seebach\" \"4\" 48:34", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RaidLevel3.png");
                Assert.AreEqual(".raids add \"Metallskulptur\" \"3\" 50:25", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RaidLevel2.png");
                Assert.AreEqual(".raids add \"Kloos Tiki\" \"2\" 47:0", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "RaidLevel1.png");
                Assert.AreEqual(".raids add \"Brunnen Sternen Oerlikon\" \"1\" 46:55", text, true);

                text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "LevelWrong3Instead5.png");
                Assert.AreEqual(".raids add \"Hasenberg\" \"5\" 10:14", text, true);
            }
        }

	    

		[TestMethod]
	    public void TimezoneTest()
	    {
		    var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
			var newYorkZone = DateTimeZoneProviders.Tzdb["America/New_York"];
			var zurichZone = DateTimeZoneProviders.Tzdb["Europe/Zurich"];
		    var newYorkTime = utcNow.WithZone(newYorkZone);
		    var zurichTime = utcNow.WithZone(zurichZone);
			var timeSpanToAdd = new TimeSpan(0,30,10);
		    var zurichPlusTimeSpan = zurichTime.Plus(Duration.FromTimeSpan(timeSpanToAdd));
	    }
    }
}

