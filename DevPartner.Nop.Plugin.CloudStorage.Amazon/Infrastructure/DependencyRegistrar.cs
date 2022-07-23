using DevPartner.Nop.Plugin.CloudStorage.Amazon.Cloud;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<BucketProvider>();
            services.AddScoped<BucketStorageProviderFactory>();
        }

        public int Order => 1;
    }
}
