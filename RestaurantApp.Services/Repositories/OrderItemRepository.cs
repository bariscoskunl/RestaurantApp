using Microsoft.EntityFrameworkCore;
using RestaurantApp.Common.Models;
using RestaurantApp.Data;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Services.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync();

        }
        public async Task AddItemToOrderAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

        }
        public async Task RemoveItemFromOrderAsync(int orderItemId)
        {
            var item = await _context.OrderItems.FindAsync(orderItemId);
            if(item != null)
            {
                int orderId = item.OrderId;
                _context.OrderItems.Remove(item);
                await _context.SaveChangesAsync();

                var remainingItemsCount = await _context.OrderItems.CountAsync(oi => oi.OrderId == orderId);
                if (remainingItemsCount == 0)
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled;
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
        public async Task UpdateItemQuantityAsync(int orderItemId, int newQuantity)
        {
            var item = await _context.OrderItems.FindAsync(orderItemId);
            if (item != null)
            {
                int orderId = item.OrderId;
                if (newQuantity <= 0)
                {
                    _context.OrderItems.Remove(item);
                    await _context.SaveChangesAsync();

                    var remainingItemsCount = await _context.OrderItems.CountAsync(oi => oi.OrderId == orderId);
                    if (remainingItemsCount == 0)
                    {
                        var order = await _context.Orders.FindAsync(orderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Cancelled;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    item.Quantity = newQuantity;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
