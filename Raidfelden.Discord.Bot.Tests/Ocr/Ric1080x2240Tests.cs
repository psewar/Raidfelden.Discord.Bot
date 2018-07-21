using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public class Ric1080X2240Tests : OcrTestsBase
    {
	    protected override string FolderName => "1080x2240";

		[TestMethod]
		public void BottomMenu1080X2240()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				var text = GetOcrResultString(ocrService, "1080x2240RaidNichtErkannt.jpg");
				Assert.AreEqual(".raids add \"Evang. Meth. Kirche Allschwilerplatz\" \"5\" 6:18", text, true);

			}
		}
	}
}
