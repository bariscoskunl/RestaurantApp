using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Services.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantApp.Web.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;

        public SidebarViewComponent(IOrderRepository orderRepository, ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Tüm masaları çekip boş olanları filtreliyoruz
            var allTables = await _tableRepository.GetAllTablesAsync();
            ViewBag.EmptyTables = allTables.Where(t => t.Status == RestaurantApp.Common.Enums.TableStatus.Empty).ToList();

            if (UserClaimsPrincipal != null && UserClaimsPrincipal.Identity != null && UserClaimsPrincipal.Identity.IsAuthenticated)
            {
                var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var activeOrder = await _orderRepository.GetActiveOrderByUserIdAsync(userId);
                    if (activeOrder != null)
                    {
                        ViewBag.ActiveTableId = activeOrder.TableId;
                        ViewBag.ActiveTableNumber = activeOrder.Table.TableNumber;
                    }
                }
            }
            return View();
        }    
    }
}
