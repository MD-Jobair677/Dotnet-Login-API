using Microsoft.AspNetCore.Authorization;

namespace BulkMail.Infrastructure.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = permission;
        }
    }
}