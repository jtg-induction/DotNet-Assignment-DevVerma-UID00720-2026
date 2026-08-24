using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.Enums;
using Assignment3.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Implementations
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly FoodOrderingContext _context;

        public ReportingRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<List<TopOrderedItemDto>> GetTopOrderedItemsAsync(
            long adminId,
            TopOrderedItemsRequestDto request)
        {
            var query = _context.OrderItems
                .AsNoTracking()
                .Where(oi =>
                    oi.Order.Status == OrderStatus.delivered.ToString());

            // If restaurantId is provided,generate data for that restaurant.
            if (request.RestaurantId.HasValue)
            {
                query = query.Where(oi =>
                    oi.Order.RestaurantId ==
                    request.RestaurantId.Value);
            }
            else
            {
                // Otherwise, only include restaurants associated with this admin.
                query = query.Where(oi =>
                    oi.Order.Restaurant.RestaurantOwners
                        .Any(ro => ro.UserId == adminId));
            }

            // Exclude requested menu items.
            if (request.ExcludeMenuItemIds != null &&
                request.ExcludeMenuItemIds.Any())
            {
                query = query.Where(oi =>
                    !request.ExcludeMenuItemIds
                        .Contains(oi.MenuItemId));
            }

            var result = await query
                .GroupBy(oi => new
                {
                    oi.MenuItemId,
                    oi.MenuItem.Name,
                    oi.MenuItem.RestaurantId,
                    RestaurantName = oi.MenuItem.Restaurant.Name
                })
                .Select(g => new TopOrderedItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Key.Name,
                    RestaurantId = g.Key.RestaurantId,
                    RestaurantName = g.Key.RestaurantName,

                    // Number of different orders containing this item.
                    OrderCount = g
                        .Select(x => x.OrderId)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(10)
                .ToListAsync();

            return result;
        }

        public async Task<List<FrequentlyBoughtTogetherDto>> GetFrequentlyBoughtTogetherAsync(int restaurantId)
        {
            var query = _context.OrderItems
                .AsNoTracking()
                .Where(oi =>
                    oi.Order.RestaurantId == restaurantId &&
                    oi.Order.Status == OrderStatus.delivered.ToString());

            var result = await (
                from oi1 in query
                join oi2 in query
                    on oi1.OrderId equals oi2.OrderId
                where oi1.MenuItemId < oi2.MenuItemId
                group new { oi1, oi2 } by new
                {
                    Item1Id = oi1.MenuItemId,
                    Item1Name = oi1.MenuItem.Name,
                    Item2Id = oi2.MenuItemId,
                    Item2Name = oi2.MenuItem.Name
                }
                into grouped
                select new FrequentlyBoughtTogetherDto
                {
                    Item1Id = grouped.Key.Item1Id,
                    Item1Name = grouped.Key.Item1Name,

                    Item2Id = grouped.Key.Item2Id,
                    Item2Name = grouped.Key.Item2Name,

                    OrderCount = grouped
                        .Select(x => x.oi1.OrderId)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .ToListAsync();

            return result;
        }
    }
}
