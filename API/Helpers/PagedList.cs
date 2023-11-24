using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PagedList<T> : List<T>
{
    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int) Math.Ceiling(count / (double) pageSize); // e.g. there are 10 items (Totalcount = 10) & the page allows 4 items (PageSize = 4), 10 / 4 = 2.5 rounded up (Match.Ceiling) = 3. Result, therea re 3 pages.
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items); // Returns a list of the items when a new PagedList() instace is created.
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync(); // db execution that counts nubmer of items
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); // How many items to skip depending on the page number e.g. if on pg 1 with 4 items per page, (1-1=0) * 4 = 0 (don't want to skip any items for the first page), if on page 3 and theres 4 items per page then (3-1=2) * 4 = 8 (skip 8 items as we're loading pg 3)
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
