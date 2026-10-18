using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            string? role = session.GetString("UserRole");
            int? userId = session.GetInt32("UserId");

            // 1. Check if user is logged in
            if (!userId.HasValue || string.IsNullOrEmpty(role))
            {
                context.Result = new JsonResult(new { message = "Unauthorized: Please log in first." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // 2. Check if specific roles are required and matched
            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new JsonResult(new { message = "Forbidden: Your role does not have permission." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}