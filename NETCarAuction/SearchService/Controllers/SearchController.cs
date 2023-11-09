using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Helpers;
using SearchService.Models;

namespace SearchService.Controllers;

public class SearchController : BaseAPIController
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();
        
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        
        switch (searchParams.OrderBy)
        {
            case("make"):
                query.Sort(sort => sort.Ascending(item => item.Make)).Sort(x => x.Ascending(item => item.Model));
                break;
            case("new"):
                query.Sort(sort => sort.Descending(item => item.CreatedAt));
                break;
            default:
                query.Sort(sort => sort.Ascending(item => item.AuctionEnd));
                break;
        }
        
        switch (searchParams.FilterBy)
        {
            case("finished"):
                query.Match(item => item.AuctionEnd < DateTime.UtcNow);
                break;
            case("endingSoon"):
                query.Match(item => item.AuctionEnd < DateTime.UtcNow.AddHours(6) && item.AuctionEnd > DateTime.UtcNow);
                break;
            default:
                query.Match(item => item.AuctionEnd > DateTime.UtcNow);
                break;
        }

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(item => item.Seller.ToUpper() == searchParams.Seller.ToUpper());
        }
        
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(item => item.Winner.ToUpper() == searchParams.Winner.ToUpper());
        }
        
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        
        var result = await query.ExecuteAsync();
        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}