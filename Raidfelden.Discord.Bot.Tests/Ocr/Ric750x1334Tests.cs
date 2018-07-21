using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric750X1334Tests : OcrTestsBase
    {
	    protected override string FolderName => "750X1334";

	    [TestMethod]
	    public void WithoutBar750X1334()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "A la Loko - Kyogre.png");
			    Assert.AreEqual(".raids add \"A la Loko\" \"Kyogre\" 8:7", text, true);

			    text = GetOcrResultString(ocrService, "Absol-Theilsiefje.png");
			    Assert.AreEqual(".raids add \"Theilsiefje Säule\" \"Absol\" 44:24", text, true);

			    text = GetOcrResultString(ocrService, "750x1334Level5Egg.png");
			    Assert.AreEqual(".raids add \"Gundeldinger Krippe\" \"5\" 28:15", text, true);

				text = GetOcrResultString(ocrService, "750x1334Level4Egg.png");
			    Assert.AreEqual(".raids add \"Big Bicycle\" \"4\" 58:5", text, true);

			    text = GetOcrResultString(ocrService, "750x1334ArenaName.png");
			    Assert.AreEqual(".raids add \"Baum am Holbeinhof\" \"Regice\" 21:25", text, true);
			}
	    }

	    [TestMethod]
	    public void WithHotspot750X1334()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "OpenHotSpot750X1334.png");
			    Assert.AreEqual(".raids add \"Regenbogen Kreis\" \"5\" 52:55", text, true);

				text = GetOcrResultString(ocrService, "Ntx750X1334WithHotspot.png");
				// This a screenshot from a different map, so no chance to find the right gym
				// But the screenshot is correctly recognized as raid image
			    Assert.AreEqual("8 Arenen gefunden die das Wortfragment \"Leonard\'s Museum\" enthalten. Bitte formuliere den Namen etwas exakter aus, maximal 4 dürfen übrig bleiben für den interaktiven Modus.", text, true);

			}
	    }
	}
}
