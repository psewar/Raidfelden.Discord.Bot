using Microsoft.Extensions.Configuration;

namespace Raidfelden.Configuration.Providers.Fences.Novabot
{
    public class NovabotGeoFencesConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new NovabotGeoFencesConfigurationProvider(this);
        }
    }
}
