using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Configuration;
using Raidfelden.Data.Monocle;
using Raidfelden.Services;

namespace Raidfelden.Discord.Bot.Tests.Ocr
{
	[TestClass]
	public abstract class OcrTestsBase
    {
	    protected OcrTestsBase()
	    {
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
		    ConfigurationService = new ConfigurationService();
		    ConnectionString = ConfigurationService.GetConnectionString("ScannerDatabase");
		    ContextOptions = new DbContextOptionsBuilder().UseMySql(ConnectionString).Options;
		}

	    protected IConfigurationService ConfigurationService { get; set; }

	    protected AppConfiguration Config { get; set; }

	    protected string ConnectionString { get; set; }

	    protected DbContextOptions ContextOptions { get; set; }

	    protected string BasePath => @"..\..\..\Ressources\Pictures\Raids\";

	    protected abstract string FolderName { get; }

	    protected string GetOcrResultString(IOcrService ocrService, string fileName)
	    {
		    var filePath = Path.Combine(BasePath, FolderName, fileName);
		    return OcrTestsHelper.GetOcrResultString(ocrService, filePath);
		}

	    protected OcrService GetOcrService(Hydro74000Context context)
	    {
		    return OcrTestsHelper.GetOcrService(ConfigurationService, context);
	    }

	    protected ServiceResponse GetOcrResult(OcrService ocrService, string fileName)
	    {
		    var filePath = Path.Combine(BasePath, FolderName, fileName);
		    return OcrTestsHelper.GetOcrResult(ocrService, filePath);
	    }
	}
}
