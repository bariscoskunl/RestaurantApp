using RestaurantApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Services.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId);
        Task AddItemToOrderAsync(OrderItem orderItem);
        Task RemoveItemFromOrderAsync(int orderItemId);
        Task UpdateItemQuantityAsync(int orderItemId, int newQuantity);
    }
}
