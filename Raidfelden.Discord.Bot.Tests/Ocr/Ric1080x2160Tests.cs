using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric1080X2160Tests : OcrTestsBase
    {
	    protected override string FolderName => "1080x2160";

	    [TestMethod]
	    public void BottomMenu1080X2160()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = GetOcrResultString(ocrService, "1080x2160Level4Instead5.png");
				Assert.AreEqual(".raids add \"des Zeichners Zeichnung\" \"5\" 23:17", text, true);

				// The gyms for the following tests do not exist, so just test the results on their own
				var interactiveResult = GetOcrResult(ocrService, "DortmundLevel5Egg.jpg");
				Assert.AreEqual(interactiveResult.Result.EggLevel.GetFirst(), 5);
				Assert.AreEqual(interactiveResult.Result.EggTimer.GetFirst(), new TimeSpan(0, 54, 17));

				interactiveResult = GetOcrResult(ocrService, "DortmundLevel4Egg.jpg");
				Assert.AreEqual(interactiveResult.Result.EggLevel.GetFirst(), 4);
				Assert.AreEqual(interactiveResult.Result.EggTimer.GetFirst(), new TimeSpan(0, 3, 19));

				interactiveResult = GetOcrResult(ocrService, "DortmundBossMeditie.jpg");
			    Assert.AreEqual(interactiveResult.Result.Pokemon.OcrValue, "Meditie");
			    Assert.AreEqual(interactiveResult.Result.RaidTimer.GetFirst(), new TimeSpan(0, 13, 40));

				interactiveResult = GetOcrResult(ocrService, "BottomMenu1080x2160Boss.png");
			    Assert.AreEqual(interactiveResult.Result.Pokemon.OcrValue, "Walraisa");
			    Assert.AreEqual(interactiveResult.Result.RaidTimer.GetFirst(), new TimeSpan(0, 35, 31));
			}
	    }
	}
}
