using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric1080X2220Tests : OcrTestsBase
    {
	    protected override string FolderName => "1080x2220";

	    [TestMethod]
	    public void BottomMenu1080X2220()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "ZurLandskrone-Karpador.jpg");
			    Assert.AreEqual(".raids add \"Zur Landskron\" \"Karpador\" 10:1", text, true);

			    text = GetOcrResultString(ocrService, "Egg1080X2220Herwig.jpg");
			    Assert.AreEqual(".raids add \"Skulptur Zwei Schwangere Frauen \" \"4\" 56:54", text, true);

			    text = GetOcrResultString(ocrService, "GalaxyS9WithMenu2.jpg");
			    Assert.AreEqual(".raids add \"Rheinfelden Bahnhof\" \"5\" 48:59", text, true);
		    }
	    }

	    [TestMethod]
	    public void BothMenu1080X2220()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "HoOh1080X2220.jpg");
			    Assert.AreEqual(".raids add \"Warmbacher Kreuz\" \"Ho-Oh\" 25:41", text, true);

			    text = GetOcrResultString(ocrService, "BothMenu1080X2220Egg.jpg");
			    Assert.AreEqual(".raids add \"Jeckenbrunnen\" \"1\" 51:3", text, true);
		    }
	    }

	    [TestMethod]
	    public void WithoutMenu1080X2220()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "Egg1080x2220.jpg");
			    Assert.AreEqual(".raids add \"Moderne Konferenz\" \"5\" 55:9", text, true);
			}
	    }
	}
}
