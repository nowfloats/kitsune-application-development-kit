using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KitsuneWebsiteCrawlerService.AWSHelpers
{
    public class AmazonS3Helper
    {
        /// <summary>
        /// Saves the given file in byte to s3 with given file name and bucket name
        /// Region APSouth1 (Mumbai)
        /// Acceleration tranfer is "On"
        /// </summary>
        /// <param name="fileNameAndPath">Total file path(don't include '/' at start)</param>
        /// <param name="fileBody">File body in bytes</param>
        /// <param name="bucketName">s3 Bucket name</param>
        /// <returns>true if success else throws exception</returns>
        public static bool SaveTheFileInS3(string awsAccessKeyId,
                                           string awsSecretAccessKey,
                                           string fileNameAndPath,
                                           byte[] fileBody,
                                           string bucketName,
                                           string contentType=null)
        {
            try
            {
                var stream = new MemoryStream(fileBody);
                var client=new AmazonS3Client(awsAccessKeyId,awsSecretAccessKey, new AmazonS3Config()
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1,
                    UseAccelerateEndpoint = true,
                });
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead,
                    Key = fileNameAndPath
                };
                request.Headers.ContentLength = fileBody.Length;
                request.AutoCloseStream = true;
                request.InputStream = stream;
                if(String.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }
                
                var response = client.PutObjectAsync(request).Result;
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                else
                    throw new Exception(String.Format("Unable to upload file : {0}, server returned status : {1}", fileNameAndPath, response.HttpStatusCode));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
