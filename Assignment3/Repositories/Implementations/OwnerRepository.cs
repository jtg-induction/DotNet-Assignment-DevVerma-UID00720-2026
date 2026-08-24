using Assignment3.Data;
using Assignment3.DTOs;
using Assignment3.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3.Repositories.Implementations
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly FoodOrderingContext _context;

        public OwnerRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<(List<OwnerOrderDto> Orders, int TotalCount)> GetOwnerOrdersAsync(
            long ownerId,
            OwnerOrderDashboardRequestDto request)
        {
            var query = _context.Orders  // only return orders whose restaurant is associated with given owner
                .AsNoTracking()
                .Where(o =>
                    o.Restaurant.RestaurantOwners
                        .Any(ro => ro.UserId == ownerId));

            if (request.OrderId.HasValue)
            {
                query = query.Where(o =>
                    o.Id == request.OrderId.Value);
            }

            if (request.RestaurantId.HasValue)
            {
                query = query.Where(o =>
                    o.RestaurantId == request.RestaurantId.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(o =>
                    o.Status == request.Status);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o =>
                    o.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.Date.AddDays(1);

                query = query.Where(o =>
                    o.CreatedAt < toDate);
            }

            var totalCount = await query.CountAsync();

            if (request.SortBy == "amount")
            {
                if (request.SortOrder == "desc")
                {
                    query = query.OrderByDescending(o => o.TotalAmount);
                }
                else
                {
                    query = query.OrderBy(o => o.TotalAmount);
                }
            }
            else
            {
                if (request.SortOrder == "desc")
                {
                    query = query.OrderByDescending(o => o.CreatedAt);
                }
                else
                {
                    query = query.OrderBy(o => o.CreatedAt);
                }
            }

            var orders = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OwnerOrderDto
                {
                    OrderId = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant.Name,
                    UserId = o.UserId,
                    Status = o.Status,
                    Amount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}
