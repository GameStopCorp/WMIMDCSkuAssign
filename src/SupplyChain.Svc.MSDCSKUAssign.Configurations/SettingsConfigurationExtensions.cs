using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace SupplyChain.Svc.MSDCSKUAssign.Configurations
{

    /// <summary>
    /// Extensions class for configurations class library
    /// </summary>
    public static class SettingsConfigurationExtensions
    {
        /// <summary>
        /// Adds application settings from local data store to IConfiguration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="appSettingsJson"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddApplicationSettings(this IConfigurationBuilder builder, string path)
        {
            return AddApplicationSettings(builder, path: path, provider: null, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddApplicationSettings(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddApplicationSettings(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddApplicationSettings(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddApplicationSettings(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }


        public static IConfigurationBuilder AddApplicationSettings(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new StoredSettingsSource
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
