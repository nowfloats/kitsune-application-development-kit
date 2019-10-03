using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Kitsune.API2.EnvConstants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils

{
    public class AmazonS3Helper
    {
        public static string SaveAssetsAndReturnObjectkey(string fileNameAndPath, byte[] fileBody, string bucketName, string clientId = null)
        {
            try
            {
                var imageStream = new MemoryStream(fileBody);
                var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
                    EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, new AmazonS3Config()
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1,
                        UseAccelerateEndpoint = true
                    });

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.CannedACL = S3CannedACL.PublicRead;
                request.Key = fileNameAndPath;
                request.Headers.ContentLength = fileBody.Length;
                request.AutoCloseStream = true;
                request.InputStream = imageStream;
                request.Metadata.Add("Cache-Control", "public, max-age=31536000");


                var response = client.PutObjectAsync(request).Result;
                return request.Key;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static bool DeleteAsset(string fileNameAndPath, string bucketName, string clientId = null)
        {
            try
            {
                var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
                    EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, new AmazonS3Config()
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1,
                        UseAccelerateEndpoint = true
                    });

                DeleteObjectRequest request = new DeleteObjectRequest();
                request.BucketName = bucketName;
                request.Key = fileNameAndPath;

                var response = client.DeleteObjectAsync(request).Result;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static IEnumerable<string> getFolderAssets(string crawlId, string bucketName, string domainUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(crawlId))
                {

                    //string bucketName = "kitsune-conversion";
                    //string domainUrl = "https://kitsune-conversion.s3-accelerate.amazonaws.com";

                    var client = new AmazonS3Client(new AmazonS3Config()
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1,
                        UseAccelerateEndpoint = true
                    });
                    ListObjectsV2Request request = new ListObjectsV2Request();
                    request.BucketName = bucketName;
                    request.Prefix = crawlId;

                    var response = client.ListObjectsV2Async(request).Result;
                    if (response != null && response.KeyCount > 0)
                    {
                        var result = response.S3Objects.Select(x => domainUrl + "/" + x.Key);
                        return result;
                    } 
                }
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception("");
                //EventLogger.Write(ex, "Failed to DeleteKitsuneAssets for file name" + key, null);
            }

        }
    }
}
