// AgriculturePlatform.API/Filters/AuthorizeWorkerFarmAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgriculturePlatform.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeWorkerFarmAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if user is a Worker (not Admin)
        var role = context.HttpContext.User.FindFirst("role")?.Value;
        var userType = context.HttpContext.User.FindFirst("userType")?.Value;

        if (role != "WORKER" && userType != "Worker")
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get farm ID from JWT token
        var userFarmId = context.HttpContext.User.FindFirst("farmId")?.Value;
        
        if (string.IsNullOrEmpty(userFarmId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Farm information not found" });
            return;
        }

        // Get farm ID from route
        var routeFarmId = context.RouteData.Values["farmId"]?.ToString();
        var queryFarmId = context.HttpContext.Request.Query["farmId"].ToString();
        var requestFarmId = routeFarmId ?? queryFarmId;

        // If farm ID is specified in request, verify ownership
        if (!string.IsNullOrEmpty(requestFarmId) && requestFarmId != userFarmId)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}