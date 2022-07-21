namespace DevPartner.Nop.Plugin.CloudStorage.Configuration
{
    public class FileProviderRuleConfig 
    {
        #region Properties

        /// <summary>
        /// A value indicating the name of the rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A value indicating the match pattern for the rule
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// A value indicating the replace pattern for the rule
        /// </summary>
        public string Replace { get; set; }

        #endregion
    }
}
