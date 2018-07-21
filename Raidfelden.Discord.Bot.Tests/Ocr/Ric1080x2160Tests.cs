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

			    //var interactiveResult = OcrTestsHelper.GetOcrResult(ocrService, basePath + "BottomMenu1080x2160Boss.png");
			    //var result = interactiveResult.InterActiveCallbacks.First().Value();
			    //// This gym does not exist in the database so just take one that does to let this test finish successfully
			    //text = result.Result.Message;
			    //Assert.AreEqual(".raids add \"Iron Snail Fountain\" \"Walraisa\" 35:31", text, true);
		    }
	    }
	}
}
