using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AWS.Services.S3Helper
{
    public class AmazonS3Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class AmazonS3File
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public class AmazonS3GetAssetResult: AmazonS3Result
    {
        public AmazonS3File File { get; set; }
    }

    public class S3UploadHelper
    {
        public static AmazonS3Result SaveAssetToS3(string awsKey, string awsSecretKey, string bucketName, string fileNameAndPath, byte[] fileBody,string contentType=null, bool isPublic = true)
        {
            try
            {
                if (String.IsNullOrEmpty(awsKey))
                    throw new ArgumentNullException(nameof(awsKey));
                if (String.IsNullOrEmpty(awsSecretKey))
                    throw new ArgumentNullException(nameof(awsSecretKey));
                if (String.IsNullOrEmpty(fileNameAndPath))
                    throw new ArgumentNullException(nameof(fileNameAndPath));
                if (String.IsNullOrEmpty(bucketName))
                    throw new ArgumentNullException(nameof(bucketName));

                var MemoryStream = new MemoryStream(fileBody);
                var client = new AmazonS3Client(awsKey, awsSecretKey, new AmazonS3Config()
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1,
                    UseAccelerateEndpoint = true
                });
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private,
                    Key = fileNameAndPath,
                };

                if (contentType != null)
                    request.Headers.ContentType = contentType;
                request.AutoCloseStream = true;
                request.InputStream = MemoryStream;

                var response = client.PutObjectAsync(request).Result;
                if(response.HttpStatusCode.Equals(HttpStatusCode.OK))
                {
                    return new AmazonS3Result { IsSuccess = true ,Message=response.ResponseMetadata?.RequestId};
                }
                else
                {
                    return new AmazonS3Result { Message = $"Error saving the file, statuscode : {response.HttpStatusCode} and AWSRequestId : {response.ResponseMetadata?.RequestId}", IsSuccess=false};
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file to s3",ex);
            }
        }

        public static AmazonS3GetAssetResult GetAssetFromS3(string awsKey, string awsSecretKey, string bucketName, string fileNameAndPath)
        {
            try
            {
                if (String.IsNullOrEmpty(awsKey))
                    throw new ArgumentNullException(nameof(awsKey));
                if (String.IsNullOrEmpty(awsSecretKey))
                    throw new ArgumentNullException(nameof(awsSecretKey));
                if (String.IsNullOrEmpty(fileNameAndPath))
                    throw new ArgumentNullException(nameof(fileNameAndPath));
                if (String.IsNullOrEmpty(bucketName))
                    throw new ArgumentNullException(nameof(bucketName));

                var client = new AmazonS3Client(awsKey, awsSecretKey, new AmazonS3Config()
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1,
                    UseAccelerateEndpoint = true
                });

                //Check if the file is exists(test if we can put this check to avoid the exception)
                //if(ExistsKey(fileNameAndPath, bucketName, client))
                //{
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileNameAndPath,

                    };
                    GetObjectResponse response = client.GetObjectAsync(request).Result;

                    if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
                    {
                        Stream responseStream = response.ResponseStream;
                        StreamReader reader = new StreamReader(responseStream);
                        string fileBody = reader.ReadToEnd();
                        AmazonS3File file = new AmazonS3File
                        {
                            Content = fileBody,
                            ContentType = response.Headers.ContentType
                        };
                        return new AmazonS3GetAssetResult { IsSuccess = true, File = file };
                    }
                    else
                    {
                        return new AmazonS3GetAssetResult { IsSuccess = false, Message = $"Error getting the file, statuscode : {response.HttpStatusCode} and AWSRequestId : {response.ResponseMetadata?.RequestId}" };
                    }
                //}
                //else
                //{
                //    return new AmazonS3GetAssetResult { IsSuccess = false, Message = $"file does not exists" };
                //}

            }
            catch (Exception ex)
            {
                throw new Exception("Error getting file from s3", ex);
            }
        }
        public static bool ExistsKey(string key, string bucketName, AmazonS3Client client)
        {

            var request = new ListObjectsV2Request();
            request.BucketName = bucketName;
            request.Prefix = key;
            request.MaxKeys = 1;

            ListObjectsV2Response response = client.ListObjectsV2Async(request).Result;

            if (response.S3Objects.Count == 0)
                return false;

            return true;
        }
        public static string SaveAssetsAndReturnObjectkey(string awsKey,string awsSecretKey,string fileNameAndPath, byte[] fileBody, string bucketName, string contentType = null)
        {
            try
            {
                var imageStream = new MemoryStream(fileBody);
                var client = new AmazonS3Client(awsKey,awsSecretKey,new AmazonS3Config()
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1,
                    UseAccelerateEndpoint = true
                });
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead,
                    Key = fileNameAndPath,
                };

                request.Headers.ContentLength = fileBody.Length;
                request.AutoCloseStream = true;
                request.InputStream = imageStream;
                if (contentType != null)
                    request.ContentType = contentType;

                var response = client.PutObjectAsync(request).Result;
                return request.Key;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IEnumerable<string> GetFolderAssets(string crawlId, string bucketName, string domainUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(crawlId))
                {
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
                throw ex;
            }

        }
    }
}
