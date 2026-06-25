using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RestaurantApp.Web.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
