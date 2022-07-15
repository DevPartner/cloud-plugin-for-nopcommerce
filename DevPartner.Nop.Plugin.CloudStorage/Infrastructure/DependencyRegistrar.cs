using Autofac;
using Autofac.Core;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Data;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using Microsoft.Extensions.Configuration;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Services.ExportImport;
using Nop.Services.Media;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_azure_blob_storage";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {

            var fileProvider = CommonHelper.DefaultFileProvider;
            var configPath = fileProvider.MapPath("~/Plugins/DevPartner.CloudStorage/pluginsettings.json");

            var configBuilder = new ConfigurationBuilder().AddJsonFile(configPath);

            var configuration = configBuilder.Build();
            var useDPImportManager = configuration["UseDPImportManager"];

            builder.RegisterType<CloudStorageInstallationService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<StoreDownloadService>().AsSelf().As<IDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<StorePictureService>().AsSelf().As<IPictureService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreContentService>().AsSelf().InstancePerLifetimeScope();

            if (!string.IsNullOrEmpty(useDPImportManager) && useDPImportManager.ToBoolean())
            {
                builder.RegisterType<DPImportManager>().AsSelf().As<IImportManager>().InstancePerLifetimeScope();
            }
            
            //cloud providers
            builder.RegisterType<CloudProviderFactory>().InstancePerLifetimeScope();
            builder.Register(c => (ICloudPictureProvider)c.Resolve<CloudProviderFactory>()
                    .Create(CloudStoragePlugin.PICTURE_PROVIDER_TYPE_NAME))
                .As<ICloudPictureProvider>()
                .InstancePerLifetimeScope();
            builder
                .Register(c => (ICloudThumbPictureProvider)c.Resolve<CloudProviderFactory>()
                    .Create(CloudStoragePlugin.THUMB_PICTURE_PROVIDER_TYPE_NAME))
                .As<ICloudThumbPictureProvider>()
                .InstancePerLifetimeScope();
            builder
                .Register(c => (ICloudDownloadProvider)c.Resolve<CloudProviderFactory>()
                    .Create(CloudStoragePlugin.DOWNLOAD_PROVIDER_TYPE_NAME))
                .As<ICloudDownloadProvider>()
                .InstancePerLifetimeScope();
            builder
                .Register(c => (ICloudContentProvider)c.Resolve<CloudProviderFactory>()
                    .Create(CloudStoragePlugin.CONTENT_PROVIDER_TYPE_NAME))
                .As<ICloudContentProvider>()
                .InstancePerLifetimeScope();
            builder.Register(c => new NullCloudStorageProviderFactory())
                .As<ICloudStorageProviderFactory>()
                .WithMetadata("SystemName", CloudStoragePlugin.NULL_CLOUD_PROVIDER_NAME);
            builder.RegisterType<FileCloudStorageProvider>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<ZipService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MovingItemService>().AsSelf().InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<PictureFileObjectContext>(CONTEXT_NAME);

            /*builder.RegisterType<EfRepository<PictureExt>>()
                .As<IRepository<PictureExt>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();*/

            builder.RegisterType<EfRepository<MovingItem>>()
                .As<IRepository<MovingItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order => 1;

        protected string BaseDirectory { get; }
    }
}
