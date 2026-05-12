using LoginSystem.Domain.Entities;
using LoginSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoginSystem.Infrastructure.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            var permissions = new List<Permission>
            {
                new Permission { Name = "User.View" },
                new Permission { Name = "User.Create" },
                new Permission { Name = "User.Update" },
                new Permission { Name = "User.Delete" },

                new Permission { Name = "Role.View" },
                new Permission { Name = "Role.Create" },
                new Permission { Name = "Role.Update" },
                new Permission { Name = "Role.Delete" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}