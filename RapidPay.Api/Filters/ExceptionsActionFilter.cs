using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using RapidPay.Domain.Exceptions;
using RapidPay.Domain.Repository;
using System.Net;

namespace RapidPay.Api.Filters
{
    public class ExceptionsFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionsFilter> _logger;

        private readonly IWebHostEnvironment _env;

        public ExceptionsFilter(ILogger<ExceptionsFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is CardsManagementException exception)
            {
                context.Result = GetBadRequestResultFromException(context, exception); 
                context.ExceptionHandled = true;
            }
            else if (_env.IsDevelopment())
            {
                var objectResult = new ObjectResult("An unhandled exception occurred.") { StatusCode = (int)HttpStatusCode.InternalServerError };
                _logger.LogError(context.Exception, objectResult.Value!.ToString());
                context.Result = objectResult;
            }
        }

        private static BadRequestObjectResult GetBadRequestResultFromException(ExceptionContext context, CardsManagementException exception)
        {
            context.ModelState.AddModelError(exception.MemberName ?? "Invalid", exception.GetMessage());
            
            var problem = new ValidationProblemDetails(context.ModelState);
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            
            var httpResult = new BadRequestObjectResult(problem);
            problem.Status = httpResult.StatusCode;

            return httpResult;
        }
    }
}
