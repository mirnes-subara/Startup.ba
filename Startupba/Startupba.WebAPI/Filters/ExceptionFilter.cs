using Startupba.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;

namespace Startupba.WebAPI.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;
        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            int statusCode;
            string message;
            string errorKey;

            if (context.Exception is NotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
                message = context.Exception.Message;
                errorKey = "userError";
                _logger.LogWarning(context.Exception, "{Message}", message);
            }
            else if (context.Exception is BusinessException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = context.Exception.Message;
                errorKey = "userError";
                _logger.LogWarning(context.Exception, "{Message}", message);
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Server side error, please check logs";
                errorKey = "ERROR";
                _logger.LogError(context.Exception, context.Exception.Message);
            }

            context.ModelState.AddModelError(errorKey, message);

            var list = context.ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .ToDictionary(x => x.Key, y => y.Value!.Errors.Select(z => z.ErrorMessage));

            // One envelope: Flutter reads errors, message, userText, and error.
            context.Result = new JsonResult(new
            {
                errors = list,
                message,
                userText = message,
                error = message
            })
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}
