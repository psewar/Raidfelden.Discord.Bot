using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raidfelden.Discord.Bot.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using Tesseract;

namespace Raidfelden.Discord.Bot.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar"))
            {
                var basePath = @"Ressources\Pictures\Raids\";
                var text = GetRaidText(basePath + "Screenshot_20180519-172950.png", engine);
                Assert.AreEqual(".raids add \"Feldschlösschen\" \"Kokowei\" 44:30", text, true);

                text = GetRaidText(basePath + "Schweizerkreuz_am_Bahnhof.png", engine);
                Assert.AreEqual(".raids add \"Schweizerkreuz am Bahnhof\" \"Flunkifer\" 41:29", text, true);

                text = GetRaidText(basePath + "RaidLevel5.png", engine);
                Assert.AreEqual(".raids add \"Water-Pacman at MFO Park\" \"5\" 46:15", text, true);

                text = GetRaidText(basePath + "RaidLevel4.png", engine);
                Assert.AreEqual(".raids add \"Bahnhof Graffiti Seebach\" \"4\" 48:34", text, true);

                text = GetRaidText(basePath + "Absol-Theilsiefje.png", engine);
                Assert.AreEqual(".raids add \"Theilsiefje Säule\" \"Absol\" 44:24", text, true);
            }
        }

        private string GetRaidText(string filePath, TesseractEngine engine)
        {
            using (var image = Image.Load(filePath))
            {
                using (var raidImage = new RaidImage<Rgba32>(image))
                {
                    var gymName = raidImage.GetFragmentString(engine, ImageFragmentType.GymName);
                    var timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.EggTimer);
                    var isRaidboss = string.IsNullOrWhiteSpace(timerValue);
                    if (isRaidboss)
                    {
                        var pokemonName = raidImage.GetFragmentString(engine, ImageFragmentType.PokemonName);
                        timerValue = raidImage.GetFragmentString(engine, ImageFragmentType.RaidTimer);
                        var timer = TimeSpan.Parse(timerValue);
                        return $".raids add \"{gymName}\" \"{pokemonName}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
                    }
                    else
                    {
                        var eggLevel = raidImage.GetFragmentString(engine, ImageFragmentType.EggLevel);
                        var timer = TimeSpan.Parse(timerValue);
                        return $".raids add \"{gymName}\" \"{eggLevel}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
                    }
                }
            }
        }
    }
}

