using Microsoft.AspNetCore.Authorization;

namespace EmsSystem.Infrastructure.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = permission;
        }
    }
}