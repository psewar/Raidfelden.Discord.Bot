using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Raidfelden.Services;
using Raidfelden.Configuration;
using Raidfelden.Data.Monocle;
using Raidfelden.Discord.Bot.Tests.Ocr;

namespace Raidfelden.Discord.Bot.Tests
{
    [TestClass]
    public class UnitTest1 :OcrTestsBase
    {
	    protected override string FolderName => "";

	    [TestMethod]
        public void TestMethod1()
        {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = GetOcrService(context);
				var text = GetOcrResultString(ocrService, "Screenshot_20180519-172950.png");
				Assert.AreEqual(".raids add \"Feldschlösschen\" \"Kokowei\" 44:30", text, true);

				text = GetOcrResultString(ocrService, "Schweizerkreuz_am_Bahnhof.png");
				Assert.AreEqual(".raids add \"Schweizerkreuz am Bahnhof\" \"Flunkifer\" 41:29", text, true);

                text = GetOcrResultString(ocrService, "RestzeitNichtAuswertbar.jpg");
                Assert.AreEqual(".raids add \"COOP in Basel\" \"5\" 56:57", text, true);
            }
        }

		[TestMethod]
		public void PokemonNameCorrections()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = GetOcrService(context);
				var text = GetOcrResultString(ocrService, "Amonitas.jpg");
				Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 20:5", text, true);

				// Raidboss blocks part of the name
				text = GetOcrResultString(ocrService, "Gundeldinger Krippe - Ho-Oh.png");
				Assert.AreEqual(".raids add \"Gundeldinger Krippe\" \"Ho-Oh\" 2:43", text, true);

				// Raidboss blocks part of the name
				text = GetOcrResultString(ocrService, "HoOzh.png");
				Assert.AreEqual(".raids add \"Flying Bicycle\" \"Ho-Oh\" 23:26", text, true);

				text = GetOcrResultString(ocrService, "Karpador.png");
				Assert.AreEqual(".raids add \"Flying Bicycle\" \"Karpador\" 4:2", text, true);

				
				text = GetOcrResultString(ocrService, "HoOhAsAmonitas.png");
				Assert.AreEqual(".raids add \"Steinmann am Rhein \" \"Ho-Oh\" 25:16", text, true);

				// Raidboss blocks part of the name -- No way this is gonna work ^^
				//text = GetOcrResult(ocrService, "BHF Pratteln - Ho-Oh.png", engine);
				//Assert.AreEqual(".raids add \"BHF Pratteln\" \"Ho-Oh\" 44:36", text, true);
			}
	    }

	    [TestMethod]
	    public void CpChecks()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = GetOcrService(context);
			    // Raidboss blocks part of the name - So use the CP information to get the right boss
				var text = GetOcrResultString(ocrService, "HoOhAsAmonitas.png");
				Assert.AreEqual(".raids add \"Steinmann am Rhein \" \"Ho-Oh\" 25:16", text, true);
			}
	    }

		[TestMethod]
	    public void GymNameCorrections()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
		    {
				var ocrService = GetOcrService(context);
				var text = GetOcrResultString(ocrService, "Jeckenbrunnen.png");
				Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"5\" 21:44", text, true);

				text = GetOcrResultString(ocrService, "Monument de Strasbourg.jpg");
				Assert.AreEqual(".raids add \"Monument de Strasbourg (Elisabethenanlage)\" \"1\" 21:32", text, true);

				text = GetOcrResultString(ocrService, "Schilderwald - Amonitas.png");
				Assert.AreEqual(".raids add \"Schilderwald\" \"Amonitas\" 1:7", text, true);
			}
	    }

		[TestMethod]
		public void IPhoneXImageSize()
	    {
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = GetOcrService(context);
				var text = GetOcrResultString(ocrService, "IPhoneX-Ei.png");
				Assert.AreEqual(".raids add \"The Gate\" \"5\" 28:42", text, true);

				text = GetOcrResultString(ocrService, "IPhoneX-Boss.png");
				Assert.AreEqual(".raids add \"The Gate\" \"Latios\" 39:35", text, true);
			}
		}

		[TestMethod]
		public void Interactive()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = GetOcrService(context);
				// Get the interactive Response
				var interactiveResult = GetOcrResult(ocrService, "Kreuz - Kyogre (Interactive).png");
				// The user clicked on the second entry
				var result = interactiveResult.InterActiveCallbacks.Skip(2).First().Value();
				// Raid successfully added
				var text = result.Result.Message;
				Assert.AreEqual(".raids add \"Kreuz \" \"Kyogre\" 8:7", text, true);
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

