using BulkMail.Domain.User.Entities;
using BulkMail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BulkMail.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new List<Role>
            {
                new Role { Name = "SuperAdmin", Description = "Full access to the system" },
                new Role { Name = "Admin", Description = "Administrative user" },
                new Role { Name = "User", Description = "Regular user" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }
}
