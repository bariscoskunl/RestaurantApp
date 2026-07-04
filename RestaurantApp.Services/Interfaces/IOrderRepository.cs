using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Services.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrderAsync();
        Task<IEnumerable<Order>> GetOrdersByTableIdAsync(int tableId);
        Task<Order?> GetActiveOrderByUserIdAsync(string userId);
        Task<Order?> GetOrderByIdWithItemsAsync(int orderId);
        Task<Order> CreateOrderAsync(Order order);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}
