using BulkMail.Domain.User.Entities;
using BulkMail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BulkMail.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var roles = new List<Role>
            {
                new Role { Name = "SuperAdmin", Description = "Full access to the system" },
                new Role { Name = "Admin", Description = "Administrative user" },
                new Role { Name = "User", Description = "Regular user" }
            };

            foreach (var role in roles)
            {
                var exists = await context.Roles
                    .AnyAsync(r => r.Name.ToLower() == role.Name.ToLower());

                if (!exists)
                    await context.Roles.AddAsync(role);
            }

            await context.SaveChangesAsync();
        }
    }
}
