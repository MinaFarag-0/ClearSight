using ClearSight.Core.Dtos.ApiResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Security.Claims;
using System.Text.Json;

namespace ClearSight.Api.CustomMiddleware
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context,
                                      AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            //if (authorizeResult.Forbidden)
            //{
            //    var verificationStatus = context.User.FindFirst("VerificationStatus")?.Value;
            //    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            //    if (role == "Doctor" && verificationStatus != "Approved")
            //    {
            //        context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        context.Response.ContentType = "application/json";

            //        var response = ApiResponse<string>.FailureResponse("You have not verified your account. Please wait for approval.");

            //        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            //        return;
            //    }
            //}
            if (authorizeResult.Forbidden)
            {
                var verificationStatus = context.User.FindFirst("VerificationStatus")?.Value;
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

                if (role == "Doctor" && verificationStatus != "Approved")
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<string>.FailureResponse(
                            "You have not verified your account. Please wait for approval.",
                                    System.Net.HttpStatusCode.Forbidden);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }

                    return;
                }
            }


            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
