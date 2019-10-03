using Kitsune.API.Model.ApiRequestModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class AuthHelper
    {
        public static string AuthorizeRequest(HttpRequest httpRequest)
        {
            var userId = httpRequest.Headers.ContainsKey("Authorization") ? httpRequest.Headers["Authorization"].ToString() : null;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            else
                return userId;
        }
    }
}
