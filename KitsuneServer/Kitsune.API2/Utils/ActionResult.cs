using Kitsune.API.Model.ApiRequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class CommonActionResult : IActionResult
    {
        private readonly CommonAPIResponse _result;

        public CommonActionResult(CommonAPIResponse result)
        {
            _result = result;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult;
            
            if (_result.Errors == null || !_result.Errors.Any())
            {
                objectResult = new ObjectResult(_result.Response)
                {
                    StatusCode = (int)_result.StatusCode
                };
            }
            else
            {
                //Log the error and exception of API
                var errorId = "";//ErrorId from the log;
                objectResult = new ObjectResult(new {  _result.Errors, ErrorId = errorId })
                {
                    StatusCode = (int)_result.StatusCode
                };
            }

            await objectResult.ExecuteResultAsync(context);
        }
    }
}
