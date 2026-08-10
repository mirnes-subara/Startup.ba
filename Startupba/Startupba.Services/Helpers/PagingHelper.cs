using System.Linq;
using Startupba.Model.SearchObjects;

namespace Startupba.Services.Helpers
{
    public static class PagingHelper
    {
        public static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, BaseSearchObject search)
        {
            var page = search.Page ?? 0;
            if (page < 0) page = 0;

            var size = search.PageSize ?? 30;
            if (size < 1) size = 30;
            if (size > BaseSearchObject.MaxPageSize) size = BaseSearchObject.MaxPageSize;
            search.PageSize = size;

            return query.Skip(page * size).Take(size);
        }
    }
}
