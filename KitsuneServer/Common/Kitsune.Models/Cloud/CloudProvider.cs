namespace Kitsune.Models.Cloud
{
    public enum CloudProvider
    {
        AWS,
        AliCloud,
        GCP,
        Azure
    }

    public class BasePluginCloudConfigurationDetails
    {
        public CloudProvider CloudProviderType;
        public string DefaultPreConfiguredDomain;
        public bool IsDefaultCloudProvider;

        public string AccountId;
    }
}
