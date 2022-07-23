using Amazon;
using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon
{
    public class AmazonCloudStorageSettings : ISettings
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string RegionEndPointSystemName 
        {
            get => RegionEndpoint.SystemName;
            set => RegionEndpoint = RegionEndpoint.GetBySystemName(value);
        }
        public RegionEndpoint RegionEndpoint { get; private set; } = RegionEndpoint.EUWest2;

        public string DomainNameForCDN { get; set; }
    }
}
