using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;

namespace RestaurantApp.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AdminController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()  // veritabanindan gelecekse asenkrondur,
        {
            var products = await _productRepository.GetAllProductsAsync();
            return View(products);
        }

        // ürün silme işlemi için iki aşamalı bir süreç izliyoruz: önce silme işlemi için onay sayfası göstermek, ardından silme işlemini gerçekleştirmek
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            { 
                return NotFound();
            }           
            return View(product);  // silme işlemi için onay sayfası göstermek istiyoruz, bu yüzden ürünü view'a gönderiyoruz
        }

        [HttpPost, ActionName("DeleteProduct")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteProductAsync(id);               
            return RedirectToAction("Index");  // silme işlemi tamamlandıktan sonra ürün listesine yönlendirmek istiyoruz
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        { 
            var categories = await _categoryRepository.GetAllCategoriesAsync(); // ürün oluşturma sayfasında kategorileri göstermek istiyoruz, bu yüzden kategorileri veritabanından çekiyoruz

            // kategorileri view'a göndermek için ViewBag kullanıyoruz
            ViewBag.Categories = categories.Select(category => new SelectListItem  // Dropdown listesi oluşturmak için SelectListItem kullanıyoruz, her bir kategori için bir SelectListItem oluşturuyoruz
            {
                Text = category.Name,
                Value = category.Id.ToString()
            }).ToList();
          return View();  
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] Product product, IFormFile imageFile)  // Fotograf da girebilir o yüzden IFormFile kullanıyoruz, [FromForm] attribute'u ile form verisi alir ve model binding yapariz
        {
            if (imageFile != null && imageFile.Length > 0)
            { 
                var fileName = Path.GetFileName(imageFile.FileName); // Dosya adını alıyoruz

                // sirasiyla dosya yolunu oluşturuyoruz, wwwroot/images klasörüne kaydetmek istiyoruz, fileName ile dosya adını birleştiriyoruz
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                { 
                    await imageFile.CopyToAsync(stream); // dosyayı belirtilen konuma kaydediyoruz
                }

                product.ImageUrl = fileName; // ürünün ImageUrl özelliğine dosya adını atıyoruz, böylece ürünün görselini göstermek istediğimizde bu dosya adını kullanabiliriz
                
            }
            if (ModelState.IsValid)
            { 
                await _productRepository.AddProductAsync(product); // ürün veritabanına ekleniyor
                return RedirectToAction("Index"); // ürün oluşturulduktan sonra ürün listesine
            }

            // Eger kullanici urun eklerken formu yanlis doldurursa tekrar view acilacak.
            // fakat bu noktada asagidaki return View(product) uzerinden acilacagi icin categories ilgili view dosyasina tasinamams olacak
            // bu nedenle asagida tekrar kategoriler sorgulanip , viewbag icine konuldu.
            var categories = await _categoryRepository.GetAllCategoriesAsync(); 
            
            ViewBag.Categories = categories.Select(category => new SelectListItem 
            {
                Text = category.Name,
                Value = category.Id.ToString()
            }).ToList();
            return View(product);

            // EditProduct işlemleri için benzer şekilde iki aşamalı bir süreç izlenebilir: önce düzenleme işlemi için onay sayfası göstermek, ardından işlemi gerçekleştirmek. Bu sayede kullanıcıların yanlışlıkla ürünleri düzenlemesi önlenmiş olur.
        }     

    }
}
