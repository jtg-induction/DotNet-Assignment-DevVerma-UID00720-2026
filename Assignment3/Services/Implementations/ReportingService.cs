using Assignment3.DTOs;
using Assignment3.DTOs.Common;
using Assignment3.Repositories.Interfaces;
using Assignment3.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment3.Services.Implementations
{
    public class ReportingService : IReportingService
    {
        private readonly IReportingRepository _reportingRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IReportRenderer _reportRenderer;

        public ReportingService(
            IReportingRepository reportingRepository,
            IRestaurantRepository restaurantRepository,
            IReportRenderer reportRenderer)
        {
            _reportingRepository = reportingRepository;
            _restaurantRepository = restaurantRepository;
            _reportRenderer = reportRenderer;
        }

        public async Task<ApiResponse<byte[]>> GetTopOrderedItemsAsync(
            long adminId,
            TopOrderedItemsRequestDto request)
        {
            request = request ?? new TopOrderedItemsRequestDto();

            if (request.RestaurantId.HasValue)
            {
                var isOwner = await _restaurantRepository.IsRestaurantOwnerAsync(
                        adminId,
                        request.RestaurantId.Value);

                if (!isOwner)
                {
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "You are not an owner of this restaurant.",
                        Data = null
                    };
                }
            }

            var data = await _reportingRepository.GetTopOrderedItemsAsync(
                    adminId,
                    request);

            var pdfBytes = _reportRenderer.RenderReport(
                "~/Reports/TopOrderedItems.trdp",
                data);

            return new ApiResponse<byte[]>
            {
                Success = true,
                Message = "Top ordered items report generated successfully.",
                Data = pdfBytes
            };
        }

        public async Task<ApiResponse<byte[]>> GetFrequentlyBoughtTogetherAsync(
            long adminId,
            int restaurantId)
        {
            var isOwner = await _restaurantRepository.IsRestaurantOwnerAsync(
                    adminId,
                    restaurantId);

            if (!isOwner)
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "You are not an owner of this restaurant.",
                    Data = null
                };
            }

            var data = await _reportingRepository
                    .GetFrequentlyBoughtTogetherAsync(restaurantId);

            var pdfBytes = _reportRenderer.RenderReport(
                    "~/Reports/FrequentlyBoughtTogether.trdp",
                    data);

            return new ApiResponse<byte[]>
            {
                Success = true,
                Message = "Frequently bought together report generated successfully.",
                Data = pdfBytes
            };
        }
    }
}
