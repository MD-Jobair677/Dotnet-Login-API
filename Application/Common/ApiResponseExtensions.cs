using Microsoft.AspNetCore.Mvc;

namespace BulkMail.Application.Common
{
    public static class ApiResponseExtensions
    {
        public static IActionResult ToResponse<T>(this ServiceResult<T> result)
        {
            if (result.Success)
            {
                return new OkObjectResult(new ApiResponse<T>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data
                });
            }

            return new ObjectResult(new ApiResponse<T?>
            {
                Success = false,
                Message = result.Message,
                Data = default
            })
            {
                StatusCode = result.ErrorStatusCode ?? StatusCodes.Status400BadRequest
            };
        }
    }
}
