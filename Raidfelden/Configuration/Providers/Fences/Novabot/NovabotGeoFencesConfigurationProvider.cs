using System.IO;
using Microsoft.Extensions.Configuration;

namespace Raidfelden.Configuration.Providers.Fences.Novabot
{
    public class NovabotGeoFencesConfigurationProvider : FileConfigurationProvider
    {
        public NovabotGeoFencesConfigurationProvider(FileConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                var counter = -1;
                var counterCoordinates = 0;
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (line.StartsWith("["))
                    {
                        counter++;
                        var namesString = line.Trim().Trim('[', ']');
                        var names = namesString.Split(',');
                        for (int i = 0; i < names.Length; i++)
                        {
                            Data.Add($"FencesConfiguration:Fences:{counter}:Names:{i}", names[i].ToLowerInvariant());
                        }
                        
                        counterCoordinates = 0;
                    }
                    else
                    {
                        var split = line.Split(',');
                        Data.Add($"FencesConfiguration:Fences:{counter}:Coordinates:{counterCoordinates}:Latitude", split[0].Trim());
                        Data.Add($"FencesConfiguration:Fences:{counter}:Coordinates:{counterCoordinates}:Longitude", split[1].Trim());
                        counterCoordinates++;
                    }
                }
            }
        }
    }
}
