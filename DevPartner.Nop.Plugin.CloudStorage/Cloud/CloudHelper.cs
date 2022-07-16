namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    public static partial class CloudHelper
    {
        #region Properties

        /// <summary>
        /// Gets or sets the default file provider
        /// </summary>
        public static ICloudStorageProvider FileProvider { get; set; }

        /// <summary>
        /// Gets or sets the default download provider
        /// </summary>
        public static ICloudStorageProvider DownloadProvider { get; set; }

        #endregion
    }
}
