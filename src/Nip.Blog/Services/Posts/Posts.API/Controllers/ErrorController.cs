using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nip.Blog.Services.Posts.API.Exceptions;

namespace Posts.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly IHostingEnvironment _environment;

        public ErrorController(ILogger<ErrorController> logger, IHostingEnvironment env)
        {
            _logger = logger;
            _environment = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            _logger.LogWarning("Unhandled exception detected");

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                Exception exceptionThatOccurred = exceptionFeature.Error;
                string routeWhereExceptionOccurred = exceptionFeature.Path;

                if (exceptionThatOccurred.GetType() == typeof(BlogPostsDomainException))
                {
                    _logger.LogWarning(exceptionThatOccurred, "Domain exception was thrown from {0}", routeWhereExceptionOccurred);

                    var problemDetails = new ValidationProblemDetails()
                    {
                        Instance = routeWhereExceptionOccurred,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Please refer to the errors property for additional details."
                    };

                    problemDetails.Errors.Add("DomainValidations", new string[] { exceptionThatOccurred.Message.ToString() });

                    return BadRequest(problemDetails);
                }
                else
                {
                    _logger.LogError(exceptionThatOccurred, "Exception was thrown from {0}", routeWhereExceptionOccurred);

                    var problemDetails = new
                    {
                        Error = exceptionThatOccurred.Message,
                        Status = StatusCodes.Status500InternalServerError,
                        Details = _environment.IsDevelopment() ? exceptionThatOccurred.ToString() : null,
                        Instance = routeWhereExceptionOccurred
                    };

                    return StatusCode((int)HttpStatusCode.InternalServerError, problemDetails);
                }
            }
            else
            {
                _logger.LogError("Could not obtain exception details");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Error = "Unknown error" });
            }
        }
    }
}