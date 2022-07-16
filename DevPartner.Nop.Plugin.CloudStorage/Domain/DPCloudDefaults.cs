using System;
using System.Collections.Generic;
using System.Text;

namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public class DPCloudDefaults
    {
        #region Consts
        public static string SchedultTaskType =
            "DevPartner.Nop.Plugin.CloudStorage.Infrastructure.UpdateTask";

        public const string NULL_CLOUD_PROVIDER_NAME = "nopCommerce";
        public const string PICTURE_PROVIDER_TYPE_NAME = "PictureStoreType";
        public const string THUMB_PICTURE_PROVIDER_TYPE_NAME = "ThumbPictureStoreType";
        public const string DOWNLOAD_PROVIDER_TYPE_NAME = "DownloadStoreType";
        public const string CONTENT_PROVIDER_TYPE_NAME = "ContentStoreType";
        /// <summary>
        /// Gets a path to the downloads
        /// </summary>
        public static string DownloadsPath => @"downloads";

        public const int SCHEDULE_TASK_SECONDS = 60;

        #endregion
    }
}
