namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public class DPCloudDefaults
    {
        #region Consts
        public static string SchedultTaskType =
            "DevPartner.Nop.Plugin.CloudStorage.Infrastructure.UpdateTask";

        public const string NULL_CLOUD_PROVIDER_NAME = "nopCommerce";
        public const string DOWNLOAD_PROVIDER_TYPE_NAME = "DownloadStoreType";
        public const string FILE_PROVIDER_TYPE_NAME = "FileStoreType";
        /// <summary>
        /// Gets a path to the downloads
        /// </summary>
        public static string DownloadsPath => @"downloads";

        public const int SCHEDULE_TASK_SECONDS = 60;

        #endregion
    }
}
