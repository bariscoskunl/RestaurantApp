using Microsoft.AspNetCore.Mvc;

namespace RestaurantApp.Web.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
