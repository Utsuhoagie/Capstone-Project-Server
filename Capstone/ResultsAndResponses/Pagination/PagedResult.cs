using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;

namespace Capstone.Responses.Pagination
{
	public class PagedResult<T>
	{
		private const int DEFAULT_PAGE_SIZE = 10;

		public List<T> Items { get; set; } = new List<T>();
		public int Count { get; private set; }

		public int CurrentPage { get; private set; }
		public int PageSize { get; private set; }

		public int TotalCount { get; private set; }
		public int TotalPages { get; private set; }

		public bool HasPrevious => CurrentPage > 1;
		public bool HasNext => CurrentPage < TotalPages;

		public PagedResult(List<T> items, int totalCount, int page, int pageSize = DEFAULT_PAGE_SIZE)
		{
			Items.AddRange(items);
			Count = Items.Count;

			CurrentPage = page;
			PageSize = pageSize;

			TotalCount = totalCount;
			TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
		}

		public PagedResult(List<T> items, int totalCount, PagingParams pagingParams)
		{
			Items.AddRange(items);
			Count = Items.Count;

			CurrentPage = pagingParams.Page;
			PageSize = pagingParams.PageSize;

			TotalCount = totalCount;
			TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
		}
	}
}
