namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public enum MovingItemStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,
        /// <summary>
        /// Processing
        /// </summary>
        Processing = 20,
        /// <summary>
        /// Success
        /// </summary>
        Succeed = 30,
        /// <summary>
        /// Interrupted
        /// </summary>
        Interrupted = 40,
        /// <summary>
        /// Failed
        /// </summary>
        Failed = 50,
    }
}
