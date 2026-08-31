using Microsoft.EntityFrameworkCore;

namespace BulkMail.Common.ResponseDtos
{
    public static class PaginationExtensions
    {
        public static async Task<(List<T> Items, PaginationMeta Meta)> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var meta = new PaginationMeta
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return (items, meta);
        }
    }
}
