using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api_details.Authorization
{
    public class AdminStatusAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var statusAdminClaim = user.FindFirst("statusAdmin")?.Value;

            if (string.IsNullOrEmpty(statusAdminClaim) || !bool.TryParse(statusAdminClaim, out var isAdmin) || !isAdmin)
            {
                context.Result = new UnauthorizedObjectResult("Только администраторы могут выполнять это действие.");
            }
        }
    }
}