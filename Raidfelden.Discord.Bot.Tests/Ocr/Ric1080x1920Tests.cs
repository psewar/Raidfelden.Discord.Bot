using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
    public class Ric1080X1920Tests : OcrTestsBase
    {
	    protected override string FolderName => "1080x1920";

	    [TestMethod]
	    public void BottomMenuBar1080X1920Correction()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    var text = GetOcrResultString(ocrService, "BottomMenuBar-Boss-1080x1920.jpg");
			    Assert.AreEqual(".raids add \"St. Johanns-Tor\" \"Walraisa\" 23:36", text, true);

			    text = GetOcrResultString(ocrService, "BottomMenuBar-Egg5-1080x1920.jpg");
			    Assert.AreEqual(".raids add \"Spalentor\" \"5\" 28:28", text, true);

			    text = GetOcrResultString(ocrService, "BottomMenuBar-Egg4-1080x1920.jpg");
			    Assert.AreEqual(".raids add \"Organspender Gedenkstein\" \"4\" 36:21", text, true);

			    text = GetOcrResultString(ocrService, "BossWithMenu1080X1920.png");
			    Assert.AreEqual(".raids add \"International Imaginary Museum\" \"Kokowei\" 34:55", text, true);

			    text = GetOcrResultString(ocrService, "GymNameNotRecognized1080X1920.jpg");
			    Assert.AreEqual(".raids add \"Kleinbasler Basiliskenbrünnchen\" \"5\" 37:51", text, true);

			    text = GetOcrResultString(ocrService, "BottomBarSmall1080x1920Level5.jpg");
			    Assert.AreEqual(".raids add \"FC Münchenstein Sportanlage AU\" \"5\" 22:24", text, true);

				// TODO: No idea how, but the gym name has to be made more readable
			    text = GetOcrResultString(ocrService, "1080x1920WrongGymNameDetected.jpg");
			    Assert.AreEqual(".raids add \"Monument de Strasbourg (Elisabethenanlage)\" \"Blitza\" 22:38", text, true);
			}
	    }

	    [TestMethod]
	    public void WithoutMenuBar1080X1920Correction()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    //TODO: Get a better picture with visible raid timer
			    //var text = GetOcrResult(ocrService, basePath + "Boss1080X2220.jpg", engine);
			    //Assert.AreEqual(".raids add \"The Gate\" \"Sniebel\" ??:31", text, true);

			    var text = GetOcrResultString(ocrService, "WithoutMenu1080x1920.jpg");
			    Assert.AreEqual(".raids add \"St. Johanns Ring Antique House\" \"2\" 40:13", text, true);
		    }
	    }

	    [TestMethod]
	    public void NullRefException()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
			    // The problem here was a to strict BinaryTreshold where the bot would not recognize the gym name anymore
			    var text = GetOcrResultString(ocrService, "NullRefException1.jpg");
			    Assert.AreEqual(".raids add \"Die Venus in den Büschen\" \"4\" 49:48", text, true);
		    }
	    }

	    [TestMethod]
	    public void EggLevels()
	    {
		    using (var context = new Hydro74000Context(ContextOptions))
		    {
			    var ocrService = GetOcrService(context);
			    var text = GetOcrResultString(ocrService, "RaidLevel5.png");
			    Assert.AreEqual(".raids add \"Water-Pacman at MFO Park\" \"5\" 46:15", text, true);

			    text = GetOcrResultString(ocrService, "RaidLevel4.png");
			    Assert.AreEqual(".raids add \"Bahnhof Graffiti Seebach\" \"4\" 48:34", text, true);

			    text = GetOcrResultString(ocrService, "RaidLevel3.png");
			    Assert.AreEqual(".raids add \"Metallskulptur\" \"3\" 50:25", text, true);

			    text = GetOcrResultString(ocrService, "RaidLevel2.png");
			    Assert.AreEqual(".raids add \"Kloos Tiki\" \"2\" 47:0", text, true);

			    text = GetOcrResultString(ocrService, "RaidLevel1.png");
			    Assert.AreEqual(".raids add \"Brunnen Sternen Oerlikon\" \"1\" 46:55", text, true);

			    text = GetOcrResultString(ocrService, "LevelWrong3Instead5.png");
			    Assert.AreEqual(".raids add \"Hasenberg\" \"5\" 10:14", text, true);
		    }
	    }
	}
}
