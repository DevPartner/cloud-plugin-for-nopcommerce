using System;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using Nop.Services.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    public class UpdateTask : IScheduleTask
    {
        private readonly StorePictureService _storePictureService;
        private readonly StoreDownloadService _storeDownloadService;
        private readonly StoreContentService _storeContentService;
        private readonly MovingItemService _movingService;
        private readonly CloudProviderFactory _providerFactory;

        public UpdateTask(StorePictureService storePictureService,
            StoreDownloadService storeDownloadService,
            MovingItemService movingPictureService,
            CloudProviderFactory providerFactory,
            StoreContentService storeContentService)
        {
            _storePictureService = storePictureService;
            _storeDownloadService = storeDownloadService;
            _movingService = movingPictureService;
            _providerFactory = providerFactory;
            _storeContentService = storeContentService;
        }

        public void Execute()
        {
            _movingService.ResetAbortedStatuses();

            MovingItem movingItem;
            while ((movingItem = _movingService.GetIdle(MovingItemTypes.Picture)) != null)
            {
                try
                {
                    var provider = _providerFactory.Create(CloudStoragePlugin.PICTURE_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);
                    _storePictureService.MovePicture(_storePictureService.GetPictureById(movingItem.EntityId.Value), provider);
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Succeed);
                }
                catch (Exception exc)
                {
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Failed);
                }
            }

            while ((movingItem = _movingService.GetIdle(MovingItemTypes.Download)) != null)
            {
                try
                {
                    var provider = _providerFactory.Create(CloudStoragePlugin.DOWNLOAD_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);

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
                    var provider = _providerFactory.Create(CloudStoragePlugin.CONTENT_PROVIDER_TYPE_NAME, movingItem.OldProviderSystemName);

                    _storeContentService.MoveFile(movingItem.FilePath, provider);
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Succeed);
                }
                catch (Exception exc)
                {
                    _movingService.UpdateStatus(movingItem, MovingItemStatus.Failed);
                }
            }
        }
    }
}
