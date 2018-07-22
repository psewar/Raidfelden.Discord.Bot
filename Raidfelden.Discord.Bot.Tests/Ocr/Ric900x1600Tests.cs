using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric900X1600Tests : OcrTestsBase
    {
	    protected override string FolderName => "900X1600";

	    [TestMethod]
	    public void BottomMenu900X1600()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
			    var text = GetOcrResultString(ocrService, "Nexus900x1600Boss.png");
			    Assert.AreEqual(".raids add \"Helix Fountain\" \"Snorunt\" 41:40", text, true);
		    }
	    }
	}
}
