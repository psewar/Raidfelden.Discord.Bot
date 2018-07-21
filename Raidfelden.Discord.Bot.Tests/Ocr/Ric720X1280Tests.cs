using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric720X1280Tests : OcrTestsBase
    {
	    protected override string FolderName => "720x1280";

	    [TestMethod]
	    public void BottomMenu720X1280()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "BottomMenu720x1280_Egg.png");
			    Assert.AreEqual(".raids add \"Triple slider\" \"2\" 43:1", text, true);

			    text = GetOcrResultString(ocrService, "BottomMenu720x1280_Boss.png");
			    Assert.AreEqual(".raids add \"Spielplatz Erlenmatt\" \"Tyracroc\" 23:12", text, true);

			    text = GetOcrResultString(ocrService, "720x1280Egg5.jpg");
			    Assert.AreEqual(".raids add \"La Tour De L\'Hôtel De L\'Europe\" \"5\" 10:4", text, true);
		    }
	    }
	}
}
