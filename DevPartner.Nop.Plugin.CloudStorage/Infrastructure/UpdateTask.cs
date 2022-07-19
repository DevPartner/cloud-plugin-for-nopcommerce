using System;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using DevPartner.Nop.Plugin.CloudStorage.Services.NopServices;
using Nop.Services.ScheduleTasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    public class UpdateTask : IScheduleTask
    {
        private readonly CloudPictureService _cloudPictureService;
        private readonly CloudDownloadService _cloudDownloadService;
        private readonly MovingItemService _movingItemService;
        private readonly CloudProviderFactory _providerFactory;

        public UpdateTask(
            CloudPictureService cloudPictureService,
            CloudDownloadService cloudDownloadService,
            MovingItemService movingItemService,
            CloudProviderFactory providerFactory)
        {
            _cloudPictureService = cloudPictureService;
            _cloudDownloadService = cloudDownloadService;
            _movingItemService = movingItemService;
            _providerFactory = providerFactory;
        }

        public async Task ExecuteAsync()
        {
            await _movingItemService.ResetAbortedStatusesAsync();

            MovingItem movingItem;
            while ((movingItem = await _movingItemService.GetIdle(MovingItemTypes.Picture)) != null)
            {
                try
                {
                    var provider = await _providerFactory.Create(DPCloudDefaults.PICTURE_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);
                    await _cloudPictureService.MovePictureAsync(await _cloudPictureService.GetPictureByIdAsync(movingItem.EntityId.Value), provider);
                    await _movingItemService.UpdateStatusAsync(movingItem, MovingItemStatus.Succeed);
                }
                catch (Exception exc)
                {
                    await _movingItemService.UpdateStatusAsync(movingItem, MovingItemStatus.Failed);
                }
            }
            /*
            while ((movingItem = _movingService.GetIdle(MovingItemTypes.Download)) != null)
            {
                try
                {
                    var provider = _providerFactory.Create(DPCloudDefaults.DOWNLOAD_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);

                    _storeDownloadService.MoveDownload(movingItem.Id, provider);
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Succeed);
                }
                catch (Exception exc)
                {
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Failed);
                }
            }

            while ((movingItem = _movingService.GetIdle(MovingItemTypes.File)) != null)
            {
                try
                {
                    var provider = _providerFactory.Create(DPCloudDefaults.CONTENT_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);

                    _storeContentService.MoveFile(movingItem.FilePath, provider);
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Succeed);
                }
                catch (Exception exc)
                {
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Failed);
                }
            }*/
        }

       
    }
}
