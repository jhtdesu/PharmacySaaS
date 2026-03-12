using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing request.");

        ProblemDetails problemDetails;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray());

            problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dữ liệu không hợp lệ",
                Detail = "Vui lòng kiểm tra lại các trường dữ liệu được gửi lên."
            };
        }
        else if (exception is ArgumentException argumentException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Yêu cầu không hợp lệ",
                Detail = argumentException.Message
            };
        }
        else if (exception is InvalidOperationException invalidOperationException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Thao tác không thể thực hiện",
                Detail = invalidOperationException.Message
            };
        }
        else if (exception is UnauthorizedAccessException unauthorizedAccessException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Không có quyền truy cập",
                Detail = unauthorizedAccessException.Message
            };
        }
        else       
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Đã xảy ra lỗi hệ thống",
                Detail = "Vui lòng thử lại sau."
            };
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}