using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetOrdersAsync();
        Task<Order> AddNewOrderAsync(CreateOrderRequest request);
        Task<List<Order>> GetOrdersForSpecificDaysAsync(int days);
    }
}
