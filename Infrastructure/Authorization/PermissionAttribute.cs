using Microsoft.AspNetCore.Authorization;

namespace LoginSystem.Infrastructure.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = permission;
        }
    }
}