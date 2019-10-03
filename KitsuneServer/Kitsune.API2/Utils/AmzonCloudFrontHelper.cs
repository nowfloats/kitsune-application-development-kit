using Amazon;
using Amazon.CertificateManager;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Kitsune.API2.EnvConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class AmzonCloudFrontHelper
    {
        private static RegionEndpoint regionEndpoint = RegionEndpoint.APSouth1;
        private static string CloudFrontLogBucketName = "nf-application-logs.s3.amazonaws.com";
        private static string CloudFrontLogBucketPrefixName = "cf-kitsune-logs/";
        private static string KitsuneLoadBalancerAliasId = "KitsuneIdentifier2-1422384853.ap-southeast-1.elb.amazonaws.com";
        private static string KitsuneLoadBalancerOriginId = "ELB-KitsuneIdentifier2-1422384853";

        /// <summary>
        /// TODO: change OriginProtocolPolicy to matchViewer
        /// Save complete request and response in logCollection
        /// </summary>
        /// <param name="customerId"></param>
        public static CreateDistributionWithTagsResponse CreateCloudFront(string customerId, string domainName)
        {
            try
            {
                var client = new AmazonCloudFrontClient(RegionEndpoint.APSouth1);

                DistributionConfigWithTags distributionConfig = new DistributionConfigWithTags()
                {
                    DistributionConfig = new DistributionConfig()
                    {
                        Aliases = new Aliases()
                        {
                            Items = new List<string>() { domainName },
                            Quantity = 1
                        },
                        Comment = customerId,
                        Enabled = true,
                        Logging = new LoggingConfig()
                        {
                            Enabled = true,
                            Bucket = CloudFrontLogBucketName,
                            Prefix = CloudFrontLogBucketPrefixName,
                            IncludeCookies = false
                        },
                        DefaultCacheBehavior = new DefaultCacheBehavior()
                        {
                            TargetOriginId = KitsuneLoadBalancerOriginId,
                            AllowedMethods = new AllowedMethods()
                            {
                                Items = new List<string>() { "GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE" },
                                Quantity = 7
                            },
                            Compress = false,
                            DefaultTTL = 86400,
                            MaxTTL = 259200,
                            MinTTL = 86400,
                            ViewerProtocolPolicy = new ViewerProtocolPolicy("allow-all"),
                            ForwardedValues = new ForwardedValues()
                            {
                                Headers = new Headers()
                                {
                                    Items = new List<string>() {
                                        "CloudFront-Forwarded-Proto", "CloudFront-Is-Desktop-Viewer", "CloudFront-Is-Mobile-Viewer",
                                        "CloudFront-Viewer-Country", "Host", "Origin", "Referer"
                                    },
                                    Quantity = 7
                                },
                                QueryString = true,
                                Cookies = new CookiePreference() { Forward = new ItemSelection("all") }
                            },
                            TrustedSigners = new TrustedSigners() { Enabled = false, Items = new List<string>() { }, Quantity = 0 }
                        },
                        Origins = new Origins()
                        {
                            Items = new List<Origin>() {
                                new Origin() {
                                    CustomOriginConfig = new CustomOriginConfig() {
                                        HTTPPort = 80,
                                        HTTPSPort = 443,
                                        OriginKeepaliveTimeout = 5,
                                        OriginReadTimeout = 30,
                                        OriginProtocolPolicy = OriginProtocolPolicy.HttpOnly,
                                        OriginSslProtocols = new OriginSslProtocols(){
                                            Items = new List<string>(){ "TLSv1", "TLSv1.1", "TLSv1.2" },
                                               Quantity = 3
                                            }
                                    },
                                    DomainName = KitsuneLoadBalancerAliasId,
                                    Id = KitsuneLoadBalancerOriginId
                                }
                            },
                            Quantity = 1
                        },
                        CallerReference = DateTime.Now.ToString()
                    },
                    Tags = new Tags()
                    {
                        Items = new List<Tag>() {
                            new Tag(){ Key = "KITSUNE-CUSTOMERID", Value = customerId }
                        }
                    }
                };

                var request = new CreateDistributionWithTagsRequest()
                {
                    DistributionConfigWithTags = distributionConfig
                };
                return client.CreateDistributionWithTagsAsync(request).Result;
            }
            catch (Exception ex)
            {
                //TODO - log exception
            }
            return null;
        }

        /// <summary>
        /// Allow only few updations to cloudfront like SSL
        /// </summary>
        /// <param name="distributionId"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static UpdateDistributionResponse UpdateCloudFront(string distributionId, string etag, ViewerCertificate viewerCertificate = null)
        {
            try
            {
                if (String.IsNullOrEmpty(distributionId) || String.IsNullOrEmpty(etag))
                    return null;

                var client = new AmazonCloudFrontClient(RegionEndpoint.APSouth1);

                var request = new UpdateDistributionRequest()
                {
                    Id = distributionId,
                    IfMatch = etag,
                    DistributionConfig = new DistributionConfig() { }
                };

                if (viewerCertificate != null)
                    request.DistributionConfig.ViewerCertificate = viewerCertificate;

                return client.UpdateDistributionAsync(request).Result;
            }
            catch (Exception ex)
            {
                //TODO - log the exception
            }
            return null;
        }

        /// <summary>
        /// Config is required while updating a Distribution
        /// </summary>
        /// <param name="distributionId"></param>
        /// <returns></returns>
        public static GetDistributionConfigResponse GetDistributionConfigResponse(string distributionId)
        {
            try
            {
                if (String.IsNullOrEmpty(distributionId))
                    return null;

                var client = new AmazonCloudFrontClient(RegionEndpoint.APSouth1);

                var request = new GetDistributionConfigRequest() {
                    Id = distributionId
                };

                return client.GetDistributionConfigAsync(request).Result;
            }
            catch (Exception ex)
            {
                //TODO - log the exception
            }
            return null;
        }
    }
}
