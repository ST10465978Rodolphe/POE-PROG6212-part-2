using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Filters;

// Put this on a controller or an action to protect a page.
//   [RequireLogin]               -> any logged-in user (otherwise redirected to the login page)
//   [RequireLogin("Organiser")]  -> only that role (otherwise redirected to "access denied")
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLoginAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireLoginAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        // Not logged in: go to the login page, and come back here afterwards
        if (!session.IsLoggedIn())
        {
            string returnUrl = context.HttpContext.Request.Path +
                               context.HttpContext.Request.QueryString;

            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        // Logged in, but with the wrong role
        if (_roles.Length > 0 && !_roles.Contains(session.GetRole()))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}