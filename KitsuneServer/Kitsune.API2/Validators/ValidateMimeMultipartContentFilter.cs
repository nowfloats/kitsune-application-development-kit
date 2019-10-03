using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
namespace Kitsune.API2.Validators
{
    public class ValidateMimeMultipartContentFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {

        }
    }
    public class ValidateUser : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var user = actionContext.Request.Headers?.Authorization?.ToString();
            if (user == null || ( !string.IsNullOrEmpty(user) && user.Length != 24))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
        }

    }
}