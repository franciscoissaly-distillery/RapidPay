using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RapidPay.Domain.Exceptions;
using System.Net;
using System.Text.Json;

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

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is CardsManagementValidationException validationException)
            {
                context.Result = GetInvalidRequestResultFromValidationException(context, validationException);
                _logger.LogError(validationException, "Invalid Request: " + validationException.GetValidationMessage());
            }
            else
            {
                var httpResult = new ContentResult()
                {
                    Content = "An unhandled exception occurred",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

                if (_env.IsDevelopment())
                {
                    httpResult.ContentType = "application/json";
                    httpResult.Content = JsonSerializer.Serialize(
                        new
                        {
                            Exception = context.Exception.GetType().Name,
                            Message = context.Exception.Message,
                            StackTrace = context.Exception.StackTrace
                        },
                        _jsonOptions);
                }

                context.Result = httpResult;

                string errorMessage = "Unhandled exception";
                if (context.Exception is HttpRequestException httpException)
                    errorMessage = "Request exception";

                _logger.LogError(context.Exception, errorMessage);
            }
            context.ExceptionHandled = true;
        }

        private static BadRequestObjectResult GetInvalidRequestResultFromValidationException(ExceptionContext context, CardsManagementValidationException exception)
        {
            context.ModelState.AddModelError(exception.MemberName ?? "Invalid", exception.GetValidationMessage());

            var problem = new ValidationProblemDetails(context.ModelState);
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

            var httpResult = new BadRequestObjectResult(problem);

            switch (exception.InvalidCategory)
            {
                case InvalidCategoryEnum.UnknownEntity:
                    httpResult.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    break;
                default:
                    break;
            }

            problem.Status = httpResult.StatusCode;
            return httpResult;
        }
    }
}
