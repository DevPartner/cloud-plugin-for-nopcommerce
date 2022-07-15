using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace DevPartner.Nop.Plugin.CloudStorage.Data
{
    public class EfStartUpTask : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add object context
            services.AddDbContext<PictureFileObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 0; }
        }
    }
}
