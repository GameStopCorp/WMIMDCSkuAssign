using Microsoft.Extensions.Configuration;
using SupplyChain.Svc.MSDCSKUAssign.Configurations.ConfigRepositories;
using System.IO;

namespace SupplyChain.Svc.MSDCSKUAssign.Configurations
{
    public class SettingsConfigurationProvider : FileConfigurationProvider
    {
        public SettingsConfigurationProvider(FileConfigurationSource source) : base(source)
        { }

        public override void Load(Stream stream)
        {
            var hydrator = new SettingConfigurationRepository(stream);
            Data = hydrator.GetSettingValues();
        }
    }
}
