using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric1440X2960Tests : OcrTestsBase
    {
	    protected override string FolderName => "1440x2960";

	    [TestMethod]
	    public void BottomMenu1440X2960()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "GalaxyS9WithMenu.jpg");
			    Assert.AreEqual(".raids add \"Einheitskreis Skulptur\" \"Amonitas\" 27:5", text, true);
		    }
	    }

	    [TestMethod]
	    public void WithoutMenu1440X2960()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "1440x2960RaidNichtErkannt.jpg");
			    Assert.AreEqual(".raids add \"ABB Toro\" \"5\" 48:27", text, true);
		    }
	    }
	}
}
