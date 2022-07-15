using Autofac;
using DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<BlobProvider>().AsSelf().InstancePerDependency();
            builder.RegisterType<BlobStorageProviderFactory>().AsSelf().As<ICloudStorageProviderFactory>()
                .WithMetadata("SystemName", AzureBlobProviderPlugin.ProviderSystemName)
                .WithMetadata("Settings", AzureBlobProviderPlugin.ComponentName);
        }

        public int Order => 1;
    }
}
