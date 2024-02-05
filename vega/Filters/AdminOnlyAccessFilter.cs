using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class AdminOnlyAccessFilter : Attribute, IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var role = context.HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
        if (role == null || role != Roles.Admin)
        {
            context.Result = new ForbidResult();
        };
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}