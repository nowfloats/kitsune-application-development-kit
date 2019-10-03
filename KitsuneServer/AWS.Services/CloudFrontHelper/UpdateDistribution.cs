using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWS.Services.CloudFrontHelper
{
    public class UpdateCloudfrontDistribution
    {
        public static string AddDomainToDistribution(string distributionId, string domainName)
        {
            try
            {
                if (String.IsNullOrEmpty(distributionId)) throw new Exception("distributionId cannot be null");
                if (String.IsNullOrEmpty(domainName)) throw new Exception("domain name cannnot be null");

                domainName= domainName.Trim(' ');
                domainName = domainName.ToLower();

                using (AmazonCloudFrontClient cfClient = new AmazonCloudFrontClient(RegionEndpoint.APSouth1))
                {
                    GetDistributionConfigRequest distributionConfig = new GetDistributionConfigRequest
                    {
                        Id = distributionId
                    };
                    var config = cfClient.GetDistributionConfigAsync(distributionConfig).Result;
                    
                    //  Set a new config
                    DistributionConfig newConfig = config.DistributionConfig;
                    if (newConfig.Aliases.Items.Contains(domainName))
                        return "already present";
                    newConfig.Aliases.Items.Add(domainName);
                    newConfig.Aliases.Quantity += 1;
                    
                    //  Update 
                    UpdateDistributionRequest req = new UpdateDistributionRequest
                    {
                        Id = distributionId,
                        IfMatch = config.ETag,
                        DistributionConfig = newConfig
                    };
                    
                    var response = cfClient.UpdateDistributionAsync(req).Result;
                }

                return "success";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string AddDomainToDistribution(string AWSAccesKey,string AWSSecretKey,string distributionId, string domainName)
        {
            try
            {
                if (String.IsNullOrEmpty(distributionId)) throw new Exception("distributionId cannot be null");
                if (String.IsNullOrEmpty(domainName)) throw new Exception("domain name cannnot be null");

                domainName = domainName.Trim(' ');
                domainName = domainName.ToLower();

                using (AmazonCloudFrontClient cfClient = new AmazonCloudFrontClient(AWSAccesKey,AWSSecretKey,RegionEndpoint.APSouth1))
                {
                    GetDistributionConfigRequest distributionConfig = new GetDistributionConfigRequest
                    {
                        Id = distributionId
                    };
                    var config = cfClient.GetDistributionConfigAsync(distributionConfig).Result;

                    //  Set a new config
                    DistributionConfig newConfig = config.DistributionConfig;
                    if (newConfig.Aliases.Items.Contains(domainName))
                        return "already present";
                    newConfig.Aliases.Items.Add(domainName);
                    newConfig.Aliases.Quantity += 1;

                    //  Update 
                    UpdateDistributionRequest req = new UpdateDistributionRequest
                    {
                        Id = distributionId,
                        IfMatch = config.ETag,
                        DistributionConfig = newConfig
                    };

                    var response = cfClient.UpdateDistributionAsync(req).Result;
                }

                return "success";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
