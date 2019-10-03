using Amazon;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
//using Kitsune.Helper;
using Kitsune.API2.EnvConstants;
using Microsoft.Extensions.Configuration;
using Kitsune.API.Model.ApiRequestModels;
//using Microsoft.Extensions.Configuration;

namespace Kitsune.API2.Utils
{

    public class AmazonS3FileProcessor
    {
        public static bool _isDev = true.Equals(EnvironmentConstants.ApplicationConfiguration.IsDev);
        public static Dictionary<string, string> AmazonS3BucketDictionary = new Dictionary<string, string>()
        {
#if !DEBUG
                    {"kitsune-content-cdn", "https://cdn.kitsune.tools"},
                    {"kitsune-conversion-prod", "https://d1y3s8cye6541c.cloudfront.net"},
                    {"webaction-files-cdn", "https://cdn.kitsune.tools"}
#else
                    {"kitsune-content-cdn", "https://cdn.kitsune.tools"},
                    { "kitsune-content-cdn-dev", "https://s3-us-west-2.amazonaws.com/kitsune-content-cdn-dev"},
                    {"kitsune-conversion", "https://d3e8v7jhu3n57l.cloudfront.net"},
                    {"webaction-files-cdn", "https://s3-ap-southeast-1.amazonaws.com/webaction-files-cdn"},
                    {AmazonAWSConstants.SourceBucketName, AmazonAWSConstants.SourceBucketUrl},
                    {AmazonAWSConstants.DemoBucketName, AmazonAWSConstants.DemoBucketUrl},
                    {AmazonAWSConstants.ProductionBucketName, AmazonAWSConstants.ProductionBucketUrl},
                    {"bucket-base-url","https://s3-ap-south-1.amazonaws.com/" }   
            #endif
        };

        public static string UploadResource(string projectId, string resourcePath, byte[] resourceBody, string clientId = null, bool isPath = false)
        {
            try
            {
                if (!String.IsNullOrEmpty(projectId))
                {
                    //Uploads only in source bucket
                    string bucketName = AmazonAWSConstants.SourceBucketName;
                    //resourcePath = resourcePath.ToLower();
                    //string folderName = ProjectId + resourceName;


                    var ObjectId = SaveAssetsAndReturnObjectkey(projectId, resourcePath, resourceBody, clientId, bucketName);

                    var domainUrl = AmazonS3BucketDictionary[bucketName];
                    var fileUrl = String.Format("{0}/{1}", domainUrl, ObjectId);

                    return fileUrl;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
            }

            return null;
        }

        //saves html pages to S3 and returns url.
        public static string SaveFileContentToS3(string ProjectId, string bucketName, string resourcePath, string fileContent, bool compiled = false, int version = 0, bool base64 = false, string clientId = null, byte[] byteArrayStream = null)
        {
            try
            {
                if (!String.IsNullOrEmpty(ProjectId))
                {
                    string folderName = ProjectId;
                    if (bucketName == AmazonAWSConstants.DemoBucketName)
                    {
                        if (version == 0)
                            folderName += "/cwd";
                        else if (version > 0)
                        {
                            folderName += "/v" + version.ToString();
                            if (compiled)
                                folderName += "/demo";
                            else
                                folderName += "/src";
                        }
                        else
                            return null;
                    }
                   
                    if (byteArrayStream == null)
                    {

                        if (base64)
                        {
                            byteArrayStream = Encoding.UTF8.GetBytes(fileContent);
                        }
                        else
                        {
                            try
                            {
                                byteArrayStream = Convert.FromBase64String(fileContent);
                            }
                            catch
                            {
                                byteArrayStream = Encoding.UTF8.GetBytes(fileContent);
                            }
                        }
                    }


                    var ObjectId = SaveAssetsAndReturnObjectkey(folderName, resourcePath, byteArrayStream, clientId, bucketName);

                    //var domainUrl = AmazonS3BucketDictionary["bucket-base-url"] + AmazonS3BucketDictionary[bucketName];
                    //var fileUrl = String.Format("{0}/{1}", domainUrl, ObjectId);

                    //return fileUrl;
                    string filePath = ObjectId.Substring(ObjectId.IndexOf(ProjectId) + ProjectId.Length);
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                throw;
                //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
                //return null;
            }

            return null;
        }
        public static string SaveFileToS3(string ProjectId, string sourceBucketName, string resourcePath, string destBucketName, bool compiled = false, int version = 0, string clientId = null)
        {
            try
            {
                if (!String.IsNullOrEmpty(ProjectId))
                {
                    var awsS3Config = new AmazonS3Config()
                    {
                        UseAccelerateEndpoint = true
                    };
                    if (_isDev)
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    else
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    //var client = new AmazonS3Client(awsS3Config);
                    var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                    string sourceKey = ProjectId;
                    string destKey = ProjectId;
                    if (sourceBucketName == AmazonAWSConstants.DemoBucketName)
                    {
                        if (version == 0)
                            sourceKey += "/cwd";
                        else if (version > 0)
                        {
                            sourceKey += "/v" + version.ToString();
                            if (compiled)
                                sourceKey += "/demo";
                            else
                                sourceKey += "/src";
                        }
                    }

                    if (destBucketName == AmazonAWSConstants.DemoBucketName)
                    {
                        if (version == 0)
                            destKey += "/cwd";
                        else if (version > 0)
                        {
                            destKey += "/v" + version.ToString();
                            if (compiled)
                                destKey += "/demo";
                            else
                                destKey += "/src";
                        }
                    }
                    sourceKey += resourcePath;
                    destKey += resourcePath;
                    CopyObjectRequest copyRequest = new CopyObjectRequest()
                    {
                        SourceBucket = sourceBucketName,
                        DestinationBucket = destBucketName,
                        SourceKey = sourceKey,
                        DestinationKey = destKey,
                        CannedACL = S3CannedACL.PublicRead,
                    };
                    CopyObjectResponse copyResponse = client.CopyObjectAsync(copyRequest).Result;
                    return destKey;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
            }

            return null;
        }

        public static string SaveAssetsAndReturnObjectkey(string folderName, string filename, byte[] fileBody, string clientId, string bucketName)
        {
            try
            {
                var imageStream = new MemoryStream(fileBody);
                //var decodedString = HttpUtility.UrlDecode(fileBody, Encoding.Default);
                //MemoryStream stream = new MemoryStream();
                //StreamWriter writer = new StreamWriter(stream);
                //writer.Write(decodedString);
                //writer.Flush();
                //stream.Position = 0;
                var awsS3Config = new AmazonS3Config()
                {
                    UseAccelerateEndpoint = true
                };
                if (_isDev)
                    awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                else
                    awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                //IAmazonS3 client;
                //var options = .GetAWSOptions();
                //IAmazonS3 client = options.CreateServiceClient();
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.CannedACL = S3CannedACL.PublicRead;
                request.Key = folderName + filename;
                request.Metadata.Add("Cache-Control", "public, max-age=31536000");


                request.AutoCloseStream = true;
                request.InputStream = imageStream;

                var response = client.PutObjectAsync(request).Result;
                return request.Key;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string getFileFromS3(string resourcePath, string ProjectId, string bucketName, bool compiled = false, int version = 0, string clientId = null)
        {
            try
            {
                if (!String.IsNullOrEmpty(resourcePath))
                {
                    string key = ProjectId;
                    if (bucketName == AmazonAWSConstants.DemoBucketName)
                    {
                        if (version == 0)
                            key += "/cwd";
                        else if (version > 0)
                        {
                            key += "/v" + version.ToString();
                            if (compiled)
                                key += "/demo";
                            else
                                key += "/src";
                        }
                        else
                            return null;
                    }
                    if (version > 0)
                    {
                        key += "/v" + version.ToString();
                    }
                    key += resourcePath;
                    var awsS3Config = new AmazonS3Config()
                    {
                        UseAccelerateEndpoint = true
                    };
                    if (_isDev)
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    else
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    //var client = new AmazonS3Client(awsS3Config);
                    var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };
                    GetObjectResponse response = client.GetObjectAsync(request).Result;
                    Stream responseStream = response.ResponseStream;
                    StreamReader reader = new StreamReader(responseStream);
                    string fileBody = reader.ReadToEnd();

                    return fileBody;
                }
            }
            catch (Exception ex)
            {
                return null;
                //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
            }

            return null;
        }

        public static KitsuneFile GetKitsuneFileFromS3(string resourcePath, string ProjectId, string bucketName)
        {
            try
            {
                if (!String.IsNullOrEmpty(resourcePath))
                {
                    string key = ProjectId;
                    key += resourcePath;
                    var awsS3Config = new AmazonS3Config()
                    {
                        UseAccelerateEndpoint = true
                    };
                    if (_isDev)
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    else
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    //var client = new AmazonS3Client(awsS3Config);
                    var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };

                    GetObjectResponse response = client.GetObjectAsync(request).Result;
                    Stream responseStream = response.ResponseStream;
                    MemoryStream memStream = new MemoryStream();
                    responseStream.CopyTo(memStream);
                    var result = memStream.ToArray();
                    string base64Data = Convert.ToBase64String(result);

                    return new KitsuneFile { Base64Data = base64Data, ContentType = response.Headers.ContentType };
                }
            }
            catch (Exception ex)
            {
                return null;
                //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
            }

            return null;
        }

        //public static string copyAuditToProd(string s3Url, string clientId = null)
        //{
        //    try
        //    {
        //        if (!String.IsNullOrEmpty(s3Url))
        //        {
        //            string bucketName = Kitsune.Helper.Constants.KitsuneContentCDN;
        //            var key = s3Url.Substring(s3Url.IndexOf(bucketName) + bucketName.Length).Trim('/');
        //            var regex = new Regex(@"\/audit\/v\d+\/");
        //            var awsS3Config = new AmazonS3Config()
        //            {
        //                UseAccelerateEndpoint = true
        //            };
        //            if (_isDev)
        //                awsS3Config.RegionEndpoint = RegionEndpoint.USWest2;
        //            else
        //                awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
        //            var client = new AmazonS3Client(awsS3Config);
        //            CopyObjectRequest copyRequest = new CopyObjectRequest()
        //            {
        //                SourceBucket = bucketName,
        //                DestinationBucket = bucketName,
        //                SourceKey = key,
        //                DestinationKey = regex.Replace(key, "/prod/", 1),
        //                CannedACL = S3CannedACL.PublicRead,
        //            };
        //            CopyObjectResponse copyResponse = client.CopyObject(copyRequest);
        //            if (copyResponse != null)
        //                return regex.Replace(s3Url, "/prod/", 1);
        //            else
        //                return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //EventLogger.Write(ex, "Failed to UploadBizImageToAmazonS3 for imageName" + themeid + "/" + filename, null);
        //    }

        //    return null;
        //}

        //public static bool CopyAuditToProd(string themeid, int themeVersion)
        //{
        //    try
        //    {
        //        string bucketName = Kitsune.Helper.Constants.KitsuneContentCDN;
        //        var auditRegex = new Regex(@"\/audit\/v(\d+)\/");
        //        List<KeyVersion> AssetDeleteList = new List<KeyVersion>();
        //        var awsS3Config = new AmazonS3Config()
        //        {
        //            //UseAccelerateEndpoint = true
        //        };
        //        if (_isDev)
        //            awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
        //        else
        //            awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
        //        var client = new AmazonS3Client(awsS3Config);

        //        ListObjectsV2Request VersionDeleteRequest = new ListObjectsV2Request
        //        {
        //            BucketName = bucketName,
        //            Prefix = themeid + "/prod/",
        //        };
        //        ListObjectsV2Response VersionDeleteResponse;
        //        do
        //        {
        //            VersionDeleteResponse = client.ListObjectsV2Async(VersionDeleteRequest).Result;
        //            if (VersionDeleteResponse.S3Objects.Count == 0)
        //                break;
        //            foreach (S3Object entry in VersionDeleteResponse.S3Objects)
        //            {
        //                KeyVersion keyVersion = new KeyVersion
        //                {
        //                    Key = entry.Key,
        //                };
        //                AssetDeleteList.Add(keyVersion);
        //            }
        //            VersionDeleteRequest.ContinuationToken = VersionDeleteResponse.NextContinuationToken;
        //        } while (VersionDeleteResponse.IsTruncated == true);
        //        var AssetDeleteResponse = DeleteMultipleObject(AssetDeleteList, bucketName);


        //        ListObjectsV2Request AssetListRequest = new ListObjectsV2Request
        //        {
        //            BucketName = bucketName,
        //            Prefix = themeid + "/audit/v" + themeVersion,
        //            Delimiter = "/source/"
        //        };
        //        ListObjectsV2Response AssetListResponse;

        //        do
        //        {
        //            AssetListResponse = client.ListObjectsV2Async(AssetListRequest).Result;
        //            if (AssetListResponse.S3Objects.Count == 0)
        //                return true;
        //            foreach (S3Object entry in AssetListResponse.S3Objects)
        //            {
        //                if (!entry.Key.EndsWith(".html") && !entry.Key.EndsWith(".html.dl") && !entry.Key.EndsWith("/") && !entry.Key.EndsWith("$"))
        //                {
        //                    CopyObjectRequest copyRequest = new CopyObjectRequest()
        //                    {
        //                        SourceBucket = bucketName,
        //                        DestinationBucket = bucketName,
        //                        SourceKey = entry.Key,
        //                        DestinationKey = auditRegex.Replace(entry.Key, "/prod/", 1),
        //                        CannedACL = S3CannedACL.PublicRead,
        //                    };
        //                    CopyObjectResponse copyResponse = client.CopyObjectAsync(copyRequest).Result;
        //                }
        //            }
        //            AssetListRequest.ContinuationToken = AssetListResponse.NextContinuationToken;
        //        } while (AssetListResponse.IsTruncated == true);

        //        return true;
        //    }
        //    catch (AmazonS3Exception amazonS3Exception)
        //    {
        //        //EventLogger.Write("Error occurred in VersionKitsuneAssets. Message:'{0}'", amazonS3Exception.Message);
        //        return false;
        //    }
        //}

        public static async Task<IEnumerable<string>> GetKitsuneAssets(string ProjectId, int version = 0)
        {
            try
            {
                if (!String.IsNullOrEmpty(ProjectId))
                {

                    //string bucketName = AmazonS3BucketDictionary[bucketName];
                    string bucketName = AmazonAWSConstants.SourceBucketName;

                    var awsS3Config = new AmazonS3Config()
                    {
                        UseAccelerateEndpoint = true
                    };
                    if (_isDev)
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    else
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    //var client = new AmazonS3Client(awsS3Config);
                    var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                    ListObjectsV2Request request = new ListObjectsV2Request
                    {
                        BucketName = bucketName,
                        //Prefix = "ThemeAssets/" + themeId + "/"
                        Prefix = ProjectId + "/"
                    };
                    ListObjectsV2Response response;
                    List<ListObjectsV2Response> responseList = new List<ListObjectsV2Response>();
                    do
                    {
                        response = await client.ListObjectsV2Async(request);
                        responseList.Add(response);
                        request.ContinuationToken = response.NextContinuationToken;
                    } while (response.IsTruncated == true);

                    if (responseList[0] != null && responseList[0].KeyCount > 0)
                    {
                        var domainUrl = AmazonS3BucketDictionary[bucketName];
                        List<string> result = new List<string>();
                        foreach (var point in responseList)
                        {
                            result.AddRange((point.S3Objects.Select(x => domainUrl + "/" + x.Key).ToList()));
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write(ex, "Failed to DeleteKitsuneAssets for file name" + key, null);
            }

            return null;
        }

        public static string DeleteKitsuneAssets(string key)
        {
            try
            {
                if (!String.IsNullOrEmpty(key))
                {
                    //TODO : Change the constant
                    string bucketName = EnvConstants.Constants.KitsuneContentCDN;

                    var awsS3Config = new AmazonS3Config()
                    {
                        UseAccelerateEndpoint = true
                    };
                    if (_isDev)
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    else
                        awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                    //var client = new AmazonS3Client(awsS3Config);
                    var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                    DeleteObjectRequest request = new DeleteObjectRequest();
                    request.BucketName = bucketName;
                    request.Key = key;

                    var response = client.DeleteObjectAsync(request).Result;
                    var resutl = response.HttpStatusCode;
                    var domainUrl = AmazonS3BucketDictionary[bucketName];
                    var imageUrl = String.Format("{0}/{1}", domainUrl, request.Key);

                    return imageUrl;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write(ex, "Failed to DeleteKitsuneAssets for file name" + key, null);
            }

            return null;
        }

        public static bool InvalidateFiles(string distributionId, List<string> arrayofpaths)
        {
            for (int i = 0; i < arrayofpaths.Count; i++)
            {
                arrayofpaths[i] = Uri.EscapeUriString(arrayofpaths[i]);
            }

            try
            {
                Amazon.CloudFront.AmazonCloudFrontClient oClient = new Amazon.CloudFront.AmazonCloudFrontClient(Amazon.RegionEndpoint.APSouth1);
                CreateInvalidationRequest oRequest = new CreateInvalidationRequest();
                oRequest.DistributionId = distributionId;
                oRequest.InvalidationBatch = new InvalidationBatch
                {
                    CallerReference = DateTime.Now.Ticks.ToString(),
                    Paths = new Paths
                    {
                        Items = arrayofpaths.ToList<string>(),
                        Quantity = arrayofpaths.Count
                    }
                };

                CreateInvalidationResponse oResponse = oClient.CreateInvalidationAsync(oRequest).Result;
                oClient.Dispose();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool VersionKitsuneResources(string projectId, int version)
        {
            try
            {
                string bucketName = EnvConstants.Constants.KitsuneContentCDN;
                var regex = new Regex(@"\/draft\/");
                var awsS3Config = new AmazonS3Config()
                {
                    UseAccelerateEndpoint = true
                };
                if (_isDev)
                    awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                else
                    awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
                //var client = new AmazonS3Client(awsS3Config);
                var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
                ListObjectsV2Request AssetListRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = projectId + "/draft/",
                };
                ListObjectsV2Response AssetListResponse;
                List<KeyVersion> AssetDeleteList = new List<KeyVersion>();
                do
                {
                    AssetListResponse = client.ListObjectsV2Async(AssetListRequest).Result;
                    if (AssetListResponse.S3Objects.Count == 0)
                        return true;
                    foreach (S3Object entry in AssetListResponse.S3Objects)
                    {
                        if (!entry.Key.EndsWith(".html") && !entry.Key.EndsWith(".html.dl") && !entry.Key.EndsWith("/") && !entry.Key.EndsWith("$"))
                        {
                            CopyObjectRequest copyRequest = new CopyObjectRequest()
                            {
                                SourceBucket = bucketName,
                                DestinationBucket = bucketName,
                                SourceKey = entry.Key,
                                DestinationKey = regex.Replace(entry.Key, "/audit/v" + version.ToString() + "/", 1),
                                CannedACL = S3CannedACL.PublicRead,
                            };
                            CopyObjectResponse copyResponse = client.CopyObjectAsync(copyRequest).Result;
                        }
                    }
                    AssetListRequest.ContinuationToken = AssetListResponse.NextContinuationToken;
                } while (AssetListResponse.IsTruncated == true);

                ListObjectsV2Request BackupSizeRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = projectId + "/audit/v",
                    Delimiter = "/"
                };
                var BackupSizeResponse = client.ListObjectsV2Async(BackupSizeRequest).Result.CommonPrefixes;
                if (BackupSizeResponse.Count > 5)
                {
                    var listOFBackup = new List<int>();
                    foreach (string response in BackupSizeResponse)
                    {
                        int versionNumber;
                        var versionString = response.Trim('/').Split('/').Last().TrimStart('v');
                        Int32.TryParse(versionString, out versionNumber);
                        listOFBackup.Add(versionNumber);
                    }
                    var versionToDelete = listOFBackup.Min();
                    ListObjectsV2Request VersionDeleteRequest = new ListObjectsV2Request
                    {
                        BucketName = bucketName,
                        Prefix = projectId + "/audit/v" + versionToDelete,
                    };
                    ListObjectsV2Response VersionDeleteResponse;
                    do
                    {
                        VersionDeleteResponse = client.ListObjectsV2Async(VersionDeleteRequest).Result;
                        foreach (S3Object entry in VersionDeleteResponse.S3Objects)
                        {
                            KeyVersion keyVersion = new KeyVersion
                            {
                                Key = entry.Key,
                            };
                            AssetDeleteList.Add(keyVersion);
                        }
                        VersionDeleteRequest.ContinuationToken = VersionDeleteResponse.NextContinuationToken;
                    } while (VersionDeleteResponse.IsTruncated == true);
                    var AssetDeleteResponse = DeleteMultipleObject(AssetDeleteList, bucketName);
                }
                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                //EventLogger.Write("Error occurred in VersionKitsuneAssets. Message:'{0}'", amazonS3Exception.Message);
                return false;
            }
        }

        public static bool DeleteMultipleObject(List<KeyVersion> keys, string bucketName)
        {
            var awsS3Config = new AmazonS3Config()
            {
                UseAccelerateEndpoint = true
            };
            if (_isDev)
                awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
            else
                awsS3Config.RegionEndpoint = RegionEndpoint.APSouth1;
            //var client = new AmazonS3Client(awsS3Config);
            var client = new AmazonS3Client(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, awsS3Config);
            DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keys
            };
            try
            {
                DeleteObjectsResponse response = client.DeleteObjectsAsync(multiObjectDeleteRequest).Result;
                //Console.WriteLine("Successfully deleted all the {0} items", response.DeletedObjects.Count);
                return true;
            }
            catch (DeleteObjectsException e)
            {
                //DeleteObjectsResponse errorResponse = e.Response;
                //EventLogger.Write("x {0}", errorResponse.DeletedObjects.Count);

                //EventLogger.Write("No. of objects successfully deleted = {0}", errorResponse.DeletedObjects.Count);
                //EventLogger.Write("No. of objects failed to delete = {0}", errorResponse.DeleteErrors.Count);

                //EventLogger.Write("Printing error data...");
                //foreach (DeleteError deleteError in errorResponse.DeleteErrors)
                //{
                //    EventLogger.Write("Object Key: {0}\t{1}\t{2}", deleteError.Key, deleteError.Code, deleteError.Message);
                //}
                return false;
            }
        }
    }

}
