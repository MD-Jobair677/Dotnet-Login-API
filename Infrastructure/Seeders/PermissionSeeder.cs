using EmsSystem.Domain.Entities;
using EmsSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmsSystem.Infrastructure.Seeders
{
    public static class PermissionSeeder
    {
        private static readonly string[] RequiredPermissions =
        {
            "User.View", "User.Create", "User.Update", "User.Delete",
            "Role.View", "Role.Create", "Role.Update", "Role.Delete",
            "Student.View", "Student.Create", "Student.Update", "Student.Delete"
        };

        public static async Task SeedAsync(AppDbContext context)
        {
            var existingNames = await context.Permissions
                .Select(p => p.Name)
                .ToListAsync();

            var missingPermissions = RequiredPermissions
                .Except(existingNames)
                .Select(name => new Permission { Name = name })
                .ToList();

            if (missingPermissions.Count == 0)
                return;

            await context.Permissions.AddRangeAsync(missingPermissions);
            await context.SaveChangesAsync();
        }
    }
}