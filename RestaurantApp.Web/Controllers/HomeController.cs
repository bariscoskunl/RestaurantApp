using Azure.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Execution;
using Newtonsoft.Json;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Common.Enums;
using RestaurantApp.Services.Repositories;
using RestaurantApp.Web.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace RestaurantApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;
        private readonly ISaleRepository _saleRepository;
        private const string CartSessionKey = "Cart";

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, ITableRepository tableRepository, ISaleRepository saleRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
            _saleRepository = saleRepository;
        }
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Waiter"))
            {
                return RedirectToAction("WaiterOrder", "Waiter");
            }

            var products = await _productRepository.GetAllProductsAsync();
            var categories = await _categoryRepository.GetActiveCategoriesAsync();           

            var viewModel = new HomeViewModel
            {
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            int count = cart.Sum(p => p.Quantity);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();

            var existingProduct = cart.FirstOrDefault(p => p.ProductId == productId);

            if (existingProduct != null)
            {
                existingProduct.Quantity++;
                existingProduct.ProductTotalPrice += product.Price;
            }
            else
            {
                cart.Add(new SaleProducts
                {
                    ProductId = productId,
                    Product = product,
                    Quantity = 1,
                    ProductPrice = product.Price,
                    ProductTotalPrice = product.Price
                });
            }
            SaveCart(cart);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
            {
                return NotFound();
            }
            return Json(new { price = product.Price });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();

            var existingProduct = cart.FirstOrDefault(p => p.ProductId == productId);

            if (existingProduct != null)
            {
                if (existingProduct.Quantity > 1)
                {
                    existingProduct.Quantity--;
                    existingProduct.ProductTotalPrice -= existingProduct.ProductPrice;
                }
                else
                {
                    cart.Remove(existingProduct);
                }
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("SaveOrder/{userId}")]
        public async Task<IActionResult> SaveOrder([FromBody] OrderRequest orderRequest, [FromRoute] string userId)
        {
            try
            {
                var cart = orderRequest.Cart;
                var tableId = orderRequest.TableNumber;

                if (cart == null || cart.Count == 0)
                {
                    return BadRequest(new { message = "Sepetiniz boş!" });
                }

                if (tableId == null || tableId < 1)
                {
                    return BadRequest(new { message = "Lütfen geçerli bir masa seçin!" });
                }

                // GÜVENLİK: Kullanıcının zaten aktif bir siparişi var mı?
                var activeOrder = await _orderRepository.GetActiveOrderByUserIdAsync(userId);
                if (activeOrder != null && activeOrder.TableId != tableId)
                {
                     // Eğer kullanıcının aktif masası varsa ve farklı bir masaya atmaya çalışıyorsa zorla kendi masasına al.
                     tableId = activeOrder.TableId;
                }

                var table = await _tableRepository.GetTableByIdAsync(tableId.Value);
                if (table == null)
                {
                    return BadRequest(new { message = "Seçtiğiniz masa geçersiz! Lütfen başka bir masa seçin." });
                }

                // Eğer yeni masa seçildiyse (aktif siparişi yoksa) masanın boş olduğundan emin ol
                if (activeOrder == null && table.Status != RestaurantApp.Common.Enums.TableStatus.Empty)
                {
                    return BadRequest(new { message = "Seçtiğiniz masa şu anda dolu! Lütfen başka bir masa seçin." });
                }

                var newOrder = new Order
                {
                    TableId = table.Id,
                    WaiterId = userId, // ÇOK KRİTİK: Siparişi veren kişiyi bağlıyoruz!
                    CreatedAt = DateTime.UtcNow,
                    Status = Common.Enums.OrderStatus.Preparing,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var item in cart)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Notes = ""
                    };
                    newOrder.OrderItems.Add(orderItem);                                 
                    
                }

                await _orderRepository.CreateOrderAsync(newOrder);
                await _tableRepository.UpdateTableStatusAsync(table.Id, RestaurantApp.Common.Enums.TableStatus.Occupied);

                return Ok(new { success = true, message = "Siparişiniz başarıyla alındı!" });


            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sipariş oluşturulurken sunucu tarafında bir hata oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodaySales()
        {
            var today = DateTime.Today;
            var sales = await _saleRepository.GetSalesByDateAsync(today);

            var salesObj = sales.Select(s => new
            {
                s.Id,
                s.SaleDate,
                s.TotalPrice,
                s.TableNumber,
                Products = s.SaleProducts.Select(sp => new
                {
                    sp.ProductId,
                    sp.Product.Name,
                    sp.ProductPrice,
                    sp.Quantity,
                    sp.ProductTotalPrice
                })
            });

            return Json(salesObj);
        }

        [HttpGet("GetTodaySales/{userId}")]
        public async Task<IActionResult> GetTodaySales([FromRoute] string userId)
        {
            // Müşteriler "Siparişler"e bastığında eski Sales verisi yerine "Aktif Siparişlerini" görmelidir.
            var allOrders = await _orderRepository.GetAllOrderAsync();
            var myActiveOrders = allOrders.Where(o => o.WaiterId == userId && 
                                                 o.Status != Common.Enums.OrderStatus.Completed && 
                                                 o.Status != Common.Enums.OrderStatus.Cancelled).ToList();

            var salesObj = myActiveOrders.Select(s => new
            {
                s.Id,
                SaleDate = s.CreatedAt,
                TotalPrice = s.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                TableNumber = s.Table?.TableNumber,
                Products = s.OrderItems.Select(sp => new
                {
                    sp.ProductId,
                    sp.Product.Name,
                    ProductPrice = sp.UnitPrice,
                    sp.Quantity,
                    ProductTotalPrice = sp.UnitPrice * sp.Quantity
                })
            });

            return Json(salesObj);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder([FromBody] EditOrderModel model)
        {
            if (model == null || model.OrderId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            var sale = await _saleRepository.GetSaleByIdAsync(model.OrderId);

            if (sale == null)
            {
                return NotFound(new {success = false, message = "Order not found"});
            }

            foreach (var item in model.Products)
            {
                var saleProduct = sale.SaleProducts.FirstOrDefault(p => p.ProductId == item.ProductId);

                if (saleProduct != null)
                {
                    saleProduct.Quantity = item.Quantity;
                    saleProduct.ProductTotalPrice = saleProduct.ProductPrice * item.Quantity;
                }
            }

            sale.TotalPrice = sale.SaleProducts.Sum(p => p.ProductTotalPrice);
            await _saleRepository.UpdateSaleAsync(sale);

            return Ok(new {success = true});
        }

        public List<SaleProducts> GetCart()
        {
            var cart = HttpContext.Session.GetString(CartSessionKey);

            return string.IsNullOrEmpty(cart) ? new List<SaleProducts>() : JsonConvert.DeserializeObject<List<SaleProducts>>(cart);
        }

        public void SaveCart(List<SaleProducts> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        public void ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CallWaiter()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız." });

            // Müşterinin aktif siparişi var mı?
            var activeOrder = await _orderRepository.GetActiveOrderByUserIdAsync(userId);
            if (activeOrder == null)
                return BadRequest(new { message = "Henüz siparişiniz yok. Önce sipariş vermelisiniz." });

            var table = await _tableRepository.GetTableByIdAsync(activeOrder.TableId);
            if (table == null)
                return BadRequest(new { message = "Masa bulunamadı." });

            // Zaten çağrılmışsa tekrar çağırmayı engelle
            if (table.Status == TableStatus.WaiterRequested)
                return Ok(new { message = "Garson zaten çağrıldı, lütfen bekleyiniz." });

            await _tableRepository.UpdateTableStatusAsync(table.Id, TableStatus.WaiterRequested);
            return Ok(new { success = true, message = "Garson çağrıldı! En kısa sürede masanıza gelecektir." });
        }

    }
}
