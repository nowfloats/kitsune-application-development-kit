using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Helper
{
    public class HttpRequest
    {
        public static int DefaultTimeOut = 60000;
        public static IRestResponse HttpStaticCrawlGetRequest(Uri uri, string userAgent = null)
        {
            try
            {
                if (uri == null)
                    throw new Exception("Error: Uri cannot be null");
                
                var client = new RestClient(uri);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                if (userAgent != null)
                    request.AddHeader("User-Agent", userAgent);
                request.ReadWriteTimeout = DefaultTimeOut;


                var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
                var httpCreateRequestObject = client.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));

                var task = taskCompletionSource.Task;
                var result = task.Wait(DefaultTimeOut);

                if (result)
                    return task.Result;
                else
                    throw new Exception("TimeOut while downloading the file");
            }
            catch (Exception ex)
            {
                if (uri != null)
                    throw new Exception(String.Format("Error: While Requesting Url : {0}", uri.AbsoluteUri), ex);
                else
                    throw ex;
            }
        }

        public static IRestResponse HttpRequestWithReadAndWriteTimeOut(Uri uri, int readAndWriteTimeOut = 30000, string userAgent = null)
        {
            try
            {
                if (uri == null)
                    throw new Exception("Error: Uri cannot be null");

                var client = new RestClient(uri);
                var request = new RestRequest(Method.GET);
                if (userAgent != null)
                    request.AddHeader("User-Agent", userAgent);
                request.AddHeader("Accept", "*/*");
                request.ReadWriteTimeout = readAndWriteTimeOut;

                var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
                var httpCreateRequestObject = client.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));

                var task = taskCompletionSource.Task;
                var result = task.Wait(readAndWriteTimeOut);

                if (result)
                    return task.Result;
                else
                    throw new Exception("TimeOut while downloading the file");
            }
            catch (Exception ex)
            {
                if (uri != null)
                    throw new Exception(String.Format("Error: While Requesting Url : {0}", uri.AbsoluteUri), ex);
                else
                    throw ex;
            }
        }

        public static IRestResponse HttpPostRequestWithTimeOut(Uri uri, int timeOut = 30000, string userAgent = null, Object requestObject = null)
        {
            try
            {
                if (uri == null)
                    throw new Exception("Error: Uri cannot be null");

                var client = new RestClient(uri);
                var request = new RestRequest(Method.POST);
                if (userAgent != null)
                    request.AddHeader("User-Agent", userAgent);
                if (requestObject != null)
                {
                    var json = request.JsonSerializer.Serialize(requestObject);
                    request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
                }
                request.AddHeader("Accept", "*/*");
                request.Timeout = timeOut;

                var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
                var httpCreateRequestObject = client.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));

                var task = taskCompletionSource.Task;
                var result = task.Wait(DefaultTimeOut);

                if (result)
                    return task.Result;
                else
                    throw new Exception("TimeOut while downloading the file");
            }
            catch (Exception ex)
            {
                if (uri != null)
                    throw new Exception(String.Format("Error: While Requesting Url : {0}", uri.AbsoluteUri), ex);
                else
                    throw ex;
            }
        }
    }
}
