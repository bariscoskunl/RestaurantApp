using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;

using Microsoft.AspNetCore.Identity;

namespace RestaurantApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Garson")]
    public class TableController : Controller
    {
        private readonly ITableRepository _tableRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TableController(
            ITableRepository tableRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            UserManager<ApplicationUser> userManager)
        {
            _tableRepository = tableRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tables = await _tableRepository.GetAllTablesAsync();
            return View(tables);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var table = await _tableRepository.GetTableByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            // Masaya ait aktif siparişleri çek (Completed ve Cancelled hariç)
            var orders = await _orderRepository.GetOrdersByTableIdAsync(id);
            var activeOrders = orders
                .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .ToList();

            var emptyOrders = activeOrders.Where(o => !o.OrderItems.Any()).ToList();
            if (emptyOrders.Any())
            {
                foreach (var emptyOrder in emptyOrders)
                {
                    await _orderRepository.UpdateOrderStatusAsync(emptyOrder.Id, OrderStatus.Cancelled);
                }
                activeOrders = activeOrders.Where(o => o.OrderItems.Any()).ToList();
            }

            // MİMARİ DOĞRU ÇÖZÜM: Rolleri Backend'de hazırlayıp Dictionary olarak yolluyoruz
            var orderRoles = new Dictionary<int, string>();
            foreach (var order in activeOrders)
            {
                if (order.Waiter != null)
                {
                    var roles = await _userManager.GetRolesAsync(order.Waiter);
                    orderRoles[order.Id] = roles.FirstOrDefault() ?? "Müşteri"; // Rolü yoksa (sade user ise) Müşteri der.
                }
                else
                {
                    orderRoles[order.Id] = "Bilinmiyor";
                }
            }
            ViewBag.OrderRoles = orderRoles;

            ViewBag.ActiveOrders = activeOrders;
            return View(table);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tableNumber)
        {
            if (!string.IsNullOrEmpty(tableNumber))
            {
                var table = new Table
                {
                    TableNumber = tableNumber,
                    Status = TableStatus.Empty
                };
                await _tableRepository.AddTableAsync(table);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TableStatus status)
        {
            // Eğer masa 'Boş' statüsüne alınıyorsa, açık kalan siparişler de kapatılmalıdır!
            if (status == TableStatus.Empty)
            {
                var orders = await _orderRepository.GetOrdersByTableIdAsync(id);
                foreach (var order in orders)
                {
                    if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
                    {
                        await _orderRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
                    }
                }
            }

            await _tableRepository.UpdateTableStatusAsync(id, status);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItemQuantity(int orderItemId, int quantity, int tableId)
        {
            await _orderItemRepository.UpdateItemQuantityAsync(orderItemId, quantity);
            return RedirectToAction(nameof(Details), new { id = tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int orderItemId, int tableId)
        {
            await _orderItemRepository.RemoveItemFromOrderAsync(orderItemId);
            return RedirectToAction(nameof(Details), new { id = tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId, int tableId)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
            return RedirectToAction(nameof(Details), new { id = tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseTable(int id)
        {
            // Tüm aktif siparişleri Completed yap
            var orders = await _orderRepository.GetOrdersByTableIdAsync(id);
            foreach (var order in orders)
            {
                if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
                {
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
                }
            }

            // Masayı boşalt
            await _tableRepository.UpdateTableStatusAsync(id, TableStatus.Empty);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _tableRepository.DeleteTableAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
