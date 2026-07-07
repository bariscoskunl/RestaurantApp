using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Services.Repositories;
using RestaurantApp.Web.DTOs;
using System.Security.Claims;

namespace RestaurantApp.Web.Controllers
{
    [Authorize(Roles = "Waiter, Admin")]
    public class WaiterController : Controller
    {
        private readonly ITableRepository _tableRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public WaiterController(ITableRepository tableRepository, IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _tableRepository = tableRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> WaiterOrder(int? tableId)
        {
            ViewBag.TableId = tableId ?? 0;
            ViewBag.Tables = await _tableRepository.GetAllTablesAsync();
            return View(await _productRepository.GetAllProductsAsync());
        }

        [HttpPost("api/waiter/place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto request)
        {
            var table = await _tableRepository.GetTableByIdAsync(request.TableId);
            if (table == null)
            {
                return NotFound("Masa bulunamadı.");
            }

            var waiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderItems = request.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Notes = item.Notes,
                UnitPrice = item.UnitPrice,
            }).ToList();

            var newOrder = new Order
            {
                TableId = table.Id,
                WaiterId = waiterId,
                Status = OrderStatus.Preparing,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems,
            };

            await _orderRepository.CreateOrderAsync(newOrder);

            await _tableRepository.UpdateTableStatusAsync(request.TableId, TableStatus.Occupied);

            return Ok(new { Message = "Sipariş başarıyla mutfağa iletildi!", OrderId = newOrder.Id });
        }
    }
}
