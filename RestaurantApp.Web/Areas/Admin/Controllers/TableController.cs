using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Services.Repositories;

namespace RestaurantApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Waiter")]
    public class TableController : Controller
    {
        private readonly ITableRepository _tableRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISaleRepository _saleRepository;

        public TableController(
            ITableRepository tableRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            UserManager<ApplicationUser> userManager,
            ISaleRepository saleRepository)
        {
            _tableRepository = tableRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userManager = userManager;
            _saleRepository = saleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool filterEmpty = false)
        {
            var tables = await _tableRepository.GetAllTablesAsync();
            
            if (filterEmpty)
            {
                tables = tables.Where(t => t.Status == TableStatus.Empty).ToList();
                ViewBag.FilterEmpty = true;
            }

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

        [HttpGet, Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, Authorize(Roles = "Admin")]
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
                await CreateSaleFromActiveOrders(id);
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

        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseTable(int id)
        {
            var table = await _tableRepository.GetTableByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            await CreateSaleFromActiveOrders(id);

            await _tableRepository.UpdateTableStatusAsync(id, TableStatus.Empty);
            return RedirectToAction("Index");
        }

        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _tableRepository.DeleteTableAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeWaiterCall(int id)
        {
            var table = await _tableRepository.GetTableByIdAsync(id);
            if (table == null) return NotFound();

            if (table.Status == TableStatus.WaiterRequested)
            {
                await _tableRepository.UpdateTableStatusAsync(id, TableStatus.Occupied);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task CreateSaleFromActiveOrders(int tableId)
        {
            var table = await _tableRepository.GetTableByIdAsync(tableId);
            if (table == null) return;

            var orders = await _orderRepository.GetOrdersByTableIdAsync(tableId);
            var activeOrders = orders
                .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .ToList();

            if (activeOrders.Any())
            {
                var allItems = activeOrders.SelectMany(o => o.OrderItems).ToList();
                var totalPrice = allItems.Sum(oi => oi.UnitPrice * oi.Quantity);

                var sale = new Sale
                {
                    SaleDate = DateTime.Now,
                    TotalPrice = totalPrice,
                    TableNumber = int.TryParse(table.TableNumber, out var tn) ? tn : (int?)null 
                };
                await _saleRepository.AddSaleAsync(sale);

                // Get the UserId for the sale (first available WaiterId, or the current user)
                var saleUserId = activeOrders.FirstOrDefault(o => !string.IsNullOrEmpty(o.WaiterId))?.WaiterId 
                                 ?? _userManager.GetUserId(User) ?? string.Empty;

                // Group items by ProductId to avoid composite key conflicts (SaleId, ProductId)
                var groupedItems = allItems.GroupBy(i => i.ProductId).Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    UnitPrice = g.First().UnitPrice,
                    ProductTotalPrice = g.Sum(i => i.UnitPrice * i.Quantity)
                });

                foreach (var item in groupedItems)
                {
                    var saleProduct = new SaleProducts
                    {
                        SaleId = sale.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ProductPrice = item.UnitPrice,
                        ProductTotalPrice = item.ProductTotalPrice,
                        UserId = saleUserId
                    };
                    await _saleRepository.AddSaleProductAsync(saleProduct);
                }

                foreach (var order in activeOrders)
                {
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
                }
            }
        }
    }
}
