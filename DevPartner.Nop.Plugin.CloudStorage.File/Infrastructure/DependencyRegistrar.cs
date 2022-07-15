using Autofac;
using DevPartner.Nop.Plugin.CloudStorage.File.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<FileProvider>().AsSelf().InstancePerDependency();
            builder.RegisterType<FileStorageProviderFactory>().AsSelf().As<ICloudStorageProviderFactory>()
                .WithMetadata("SystemName", FileProviderPlugin.ProviderSystemName)
                .WithMetadata("Settings", FileProviderPlugin.ComponentName);
        }

        public int Order => 1;
    }
}
