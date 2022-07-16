#region Copyright
/* Copyright (C) 2017 Dev Partner LLC - All Rights Reserved. 
 *
 * This file is part of DevPartner.Search.
 * 
 * DevPartner.Search can not be copied and/or distributed without the express
 * permission of Dev Partner LLC
 *
 * Written by Kanstantsin Ivinki, March 2017
 * Email: info@dev-partner.biz
 * Website: http://dev-partner.biz
 */
#endregion

using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    public class CloudStartUpTask : INopStartup
    {
        #region Methods
      
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public void Configure(IApplicationBuilder application)
        {
            Task.Run(async() =>
            {
                var providerFactory = EngineContext.Current.Resolve<CloudProviderFactory>();
                CloudHelper.FileProvider = await providerFactory
                        .Create(DPCloudDefaults.PICTURE_PROVIDER_TYPE_NAME);
                CloudHelper.DownloadProvider = await providerFactory
                        .Create(DPCloudDefaults.DOWNLOAD_PROVIDER_TYPE_NAME);
            });

        }
        #endregion

        #region Properties
        public int Order => 1;
        #endregion
    }
}
