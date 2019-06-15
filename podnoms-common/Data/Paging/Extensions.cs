using System;
using System.Linq;

namespace PodNoms.Common.Data.Paging {
    public static class PagingExtensions {
        public static PagedResult<T> GetPaged<T> (this IQueryable<T> query,
            int page, int pageSize, int totalCount = 0) where T : class {
            var result = new PagedResult<T> ();
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.RowCount = totalCount != 0 ? totalCount : query.Count ();

            var pageCount = (double) result.RowCount / pageSize;

            var skip = (page - 1) * pageSize;
            if (totalCount != 0) {
                result.Results = query.ToList ();
            } else {
                result.Results = query.Skip (skip).Take (pageSize).ToList ();
            }
            return result;
        }
    }
}
