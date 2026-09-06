using System.Linq;
using Startupba.Model.SearchObjects;

namespace Startupba.Services.Helpers
{
    public static class PagingHelper
    {
        public static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, BaseSearchObject search)
        {
            var (page, size) = Clamp(search.Page, search.PageSize);
            search.Page = page;
            search.PageSize = size;
            return query.Skip(page * size).Take(size);
        }

        public static (int Page, int PageSize) Clamp(int? page, int? pageSize, int defaultSize = 30)
        {
            var p = page ?? 0;
            if (p < 0) p = 0;

            var size = pageSize ?? defaultSize;
            if (size < 1) size = defaultSize;
            if (size > BaseSearchObject.MaxPageSize) size = BaseSearchObject.MaxPageSize;

            return (p, size);
        }
    }
}
