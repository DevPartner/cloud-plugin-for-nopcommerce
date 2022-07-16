using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using DevPartner.Nop.Plugin.CloudStorage.Services.Nop;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Media;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            /*
            var fileProvider = CommonHelper.DefaultFileProvider;
            var configPath = fileProvider.MapPath("~/Plugins/DevPartner.CloudStorage/pluginsettings.json");

            var configBuilder = new ConfigurationBuilder().AddJsonFile(configPath);

            var configuration = configBuilder.Build();
            var useDPImportManager = configuration["UseDPImportManager"];
            */

            //Services
            services.AddScoped<ZipService>();

            //override nopCommerce services
            services.AddScoped<IDownloadService, CloudDownloadService>()
                .AddScoped<CloudDownloadService>();
            services.AddScoped<INopFileProvider, CloudFileProvider>()
                .AddScoped<CloudFileProvider>();
            services.AddScoped<IPictureService, CloudPictureService>()
                .AddScoped<CloudPictureService>();

            //Factories
            services.AddScoped<CloudProviderFactory>();
            services.AddScoped<NullCloudStorageProviderFactory>();

            //DataLayout Services
            services.AddScoped<MovingItemService>();


        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }


        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 1;

    }
}
