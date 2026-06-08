// AgriculturePlatform.API/Filters/AuthorizeFarmAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgriculturePlatform.API.Filters;

/// <summary>
/// Authorization filter that ensures users can only access resources belonging to their own farm.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeFarmAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _farmIdParamName;

    public AuthorizeFarmAttribute(string farmIdParamName = "farmId")
    {
        _farmIdParamName = farmIdParamName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get farm ID from JWT token
        var userFarmId = context.HttpContext.User.FindFirst("farmId")?.Value;
        
        if (string.IsNullOrEmpty(userFarmId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Farm information not found" });
            return;
        }

        // Get farm ID from route data
        var routeFarmId = context.RouteData.Values[_farmIdParamName]?.ToString();
        
        // Get farm ID from query string
        var queryFarmId = context.HttpContext.Request.Query[_farmIdParamName].ToString();
        
        var requestFarmId = routeFarmId ?? queryFarmId;

        // If farm ID is specified in request, verify ownership
        if (!string.IsNullOrEmpty(requestFarmId) && requestFarmId != userFarmId)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}