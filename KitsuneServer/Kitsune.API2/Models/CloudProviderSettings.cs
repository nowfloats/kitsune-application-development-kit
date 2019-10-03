using Kitsune.Models.Cloud;

namespace Kitsune.API2.Models
{
    public class CloudProviderSettings
    {
        public CloudProvider provider;
    }

    public class AliCloudProviderSettings : CloudProviderSettings
    {
        public string accountId;
        public string key;
        public string region;
        public string secret;

        public AliCloudProviderSettings()
        {
            this.provider = CloudProvider.AliCloud;
        }
    }

    public class AWSCloudProviderSettings : CloudProviderSettings
    {
        public string accountId;
        public string key;
        public string region;
        public string secret;

        public AWSCloudProviderSettings()
        {
            this.provider = CloudProvider.AWS;
        }
    }

    public class GCPProviderSettings : CloudProviderSettings
    {
        public string secret;

        public GCPProviderSettings()
        {
            this.provider = CloudProvider.GCP;
        }
    }

    public class CloudProviderFactory
    {
        public static CloudProviderSettings GetInstance(CloudProvider provider)
        {
            switch (provider)
            {
                case CloudProvider.AliCloud:
                    AliCloudProviderSettings settings = new AliCloudProviderSettings();
                    return settings;
                case CloudProvider.GCP:
                    GCPProviderSettings gcpSettings = new GCPProviderSettings();
                    return gcpSettings;
                default:
                    CloudProviderSettings defaultSettings = new CloudProviderSettings();
                    defaultSettings.provider = provider;
                    return defaultSettings;
            }
        }
    }
}
