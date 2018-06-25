using System.Globalization;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Services;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests
{
    [TestClass]
	public class NtxMapTests
    {
		protected IConfigurationService ConfigurationService { get; set; }

		public string ConnectionString { get; set; }

		public DbContextOptions ContextOptions { get; set; }

		private string basePath = @"Ressources\Pictures\Raids\Ntx\";

		public NtxMapTests()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            ConfigurationService = new ConfigurationService();
            ConnectionString = ConfigurationService.GetConnectionString("ScannerDatabase");
			ContextOptions = new DbContextOptionsBuilder().UseMySql(ConnectionString).Options;
			
		}

		[TestMethod]
		public void Ntx_BottomMenu1080X1920()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "1080x1920_BottomMenu_CumberlandPresbyterianChurch_Kyogre.png");
				Assert.AreEqual(".raids add \"Cumberland Presbyterian Church\" \"Kyogre\" 44:15", text, true);
			}
		}

		[TestMethod]
		public void Ntx_WithoutMenu750X1334()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "750x1334_SundownRanchLake_5.png");
				Assert.AreEqual(".raids add \"Sundown Ranch Lake\" \"5\" 44:15", text, true);
			}
		}

		[TestMethod]
		public void Ntx_BottomMenu1080X2160()
		{
			using (var context = new Hydro74000Context(ContextOptions))
			{
				var ocrService = OcrTestsHelper.GetOcrService(ConfigurationService, context);
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
				var text = OcrTestsHelper.GetOcrResultString(ocrService, basePath + "BottomMenu1080x2160Level1Instead2.png");
				Assert.AreEqual(".raids add \"Sundown Ranch Lake\" \"5\" 44:15", text, true);
			}
		}
	}
}
