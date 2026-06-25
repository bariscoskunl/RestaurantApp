using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Services.Repositories;

namespace RestaurantApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TableController : Controller
    {
        private readonly ITableRepository _tableRepository;

        public TableController(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index()
        {
            var tables = await _tableRepository.GetAllTablesAsync();
            return View(tables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tableNumber)
        {
            if(!string.IsNullOrEmpty(tableNumber))
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
    }
}
