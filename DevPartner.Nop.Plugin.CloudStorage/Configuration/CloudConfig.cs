using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Configuration
{
    public class CloudConfig : IConfig
    {

        #region Properties

        /// <summary>
        /// A value indicating rules for provider replacement
        /// </summary>
        public FileProviderRuleConfig[] FileProviderRuleConfig { get; set; }

        /// <summary>
        /// Gets an order of configuration
        /// </summary>
        /// <returns>Order</returns>
        public int GetOrder() => 2;

        #endregion
    }
}
