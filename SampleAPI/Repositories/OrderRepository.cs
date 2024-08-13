using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SampleApiDbContext _context;

        public OrderRepository(SampleApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.EntryDate > DateTime.UtcNow.AddDays(-1) && !o.IsDeleted)
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }

        public async Task<Order> AddNewOrderAsync(CreateOrderRequest request)
        {

            var newOrder = new Order
            {
                OrderName = request.Name,
                OrderDescription = request.Description,
                IsInvoiced = request.IsInvoiced
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            return newOrder;
        }

        public async Task<List<Order>> GetOrdersForSpecificDaysAsync(int days)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = CalculateStartDate(days);

            return await _context.Orders
                .Where(o => o.EntryDate >= startDate && o.EntryDate <= endDate && !o.IsDeleted)
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }

        private DateTime CalculateStartDate(int days)
        {
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-days);
   
            while (IsWeekend(startDate) || IsHoliday(startDate))
            {
                startDate = startDate.AddDays(-1);
            }
            return startDate;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool IsHoliday(DateTime date)
        {
            //I am creating this hardcoded calendar.
            var holidays = new List<DateTime>
            {
                new DateTime(date.Year, 8, 15), 
                new DateTime(date.Year, 8, 19)
            };

            return holidays.Contains(date.Date);
        }

    }
}
