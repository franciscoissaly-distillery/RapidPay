using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RapidPay.Domain.Exceptions;
using RapidPay.Domain.Repository;
using System.Net;

namespace RapidPay.Api.Filters
{
    public class ExceptionsFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionsFilter> _logger;

        public ExceptionsFilter(ILogger<ExceptionsFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is CardsManagementException exception)
            {
                context.Result = new BadRequestObjectResult(exception.GetMessage());
            }
            else
            {
                var objectResult = new ObjectResult("An unhandled exception occurred.") { StatusCode = (int) HttpStatusCode.InternalServerError };
                _logger.LogError(context.Exception, objectResult.Value.ToString());
                context.Result = objectResult;
            }
            context.ExceptionHandled = true;
        }
    }
}
