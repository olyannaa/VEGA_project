using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text;

public class DesiredUserInfoFilter : Attribute, IActionFilter
{
    private readonly string[]? _qualities;

    public DesiredUserInfoFilter()
    {
        _qualities = null;
    }

    public DesiredUserInfoFilter(params string[] qualities)
    {
        _qualities = qualities;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_qualities != null)
        {
            var resultStr = new StringBuilder();
            foreach (var quality in _qualities)
            {
                var value = context.HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
                if (value == null)
                {
                    resultStr.Append($"{quality} info is not provided\n");
                }
            }
            if (resultStr.Length != 0)
            {
                context.Result = new NotFoundObjectResult(resultStr.ToString());
            }
        }
    }
}