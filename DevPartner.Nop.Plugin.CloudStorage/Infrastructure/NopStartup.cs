using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Configuration;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using DevPartner.Nop.Plugin.CloudStorage.Services.NopServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
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

            /*

            1. static files requests. try simple approuce Strathweb.AspNetCore.AzureBlobFileProvider
            foreach (var rule in rules)
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = _cloudProvidersFactory.Create(rule),
                RequestPath = rule.RequestPath //"/files"
            });

            2.Saving images via PictureServices.How to add additioanal Process
            services.AddCloud().
                    .AddProvider<CloudFileProvider>()
                                .AddProcessor<ResizeWebProcessor>()
                                .AddProcessor<QualityWebProcessor>()
                                .AddProcessor<AutoOrientWebProcessor>();
	                .AddProvider<CloudDownloadsProvider>()
                                .AddProcessor<ZipProcessor>();

            3.GetPicturesURL event
               	.AddProcessor<GenerateThubnailsProcessor>(); // default nopCommerce implementatin
   	            .AddProcessor<WatermarkProcessor>();
   	            .AddProcessor<ConvertWebPProcessor>();

	        //Using microservises
   	        .AddProcessor<URLForExternalserver>(string urlFormat="{0}");

            3. Saving Files event
   	            .AddProcessor<ZipProcessor>(); 

            3. Get Files event
   	            .AddProcessor<UnZipProcessor>(); */

            //Factories
            services.AddScoped<CloudProviderFactory>();
            services.AddScoped<NullCloudStorageProviderFactory>();

            //DataLayout Services
            services.AddScoped<MovingItemService>();

            //Config
            services.AddSingleton(x => Singleton<AppSettings>.Instance.Get<CloudConfig>());
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
        public int Order => 3000;
    }
}
