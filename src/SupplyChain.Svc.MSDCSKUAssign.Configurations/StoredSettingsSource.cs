using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using SupplyChain.Svc.MSDCSKUAssign.Configurations.ConfigRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Configurations
{
    public class StoredSettingsSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new SettingsConfigurationProvider(this);
        }
    }
}
