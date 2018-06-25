using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Raidfelden.Configuration.Providers.Fences.Novabot
{
    public static class NovabotGeoFencesConfigurationExtensions
    {
        public static IConfigurationBuilder AddNovabotGeoFencesFile(this IConfigurationBuilder builder, string path)
        {
            return AddNovabotGeoFencesFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddNovabotGeoFencesFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddNovabotGeoFencesFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddNovabotGeoFencesFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddNovabotGeoFencesFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddNovabotGeoFencesFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new NovabotGeoFencesConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            return builder;
        }
    }
}
