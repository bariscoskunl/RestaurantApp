using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Common.Models;
using RestaurantApp.Web.Models;

namespace RestaurantApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(); // Role manager'ı alıyoruz
            _userManager = userManager;  // User manager'ı alıyoruz
            _signInManager = signInManager;  // Sign-in manager'ı alıyoruz     // Bu DI islemlerini Identity framework'ün sağladığı servisler üzerinden yapıyoruz  (Paket dahilinde)
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync("User")) // "User" rolünün var olup olmadığını kontrol ediyoruz
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _userManager.FindByEmailAsync(model.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password); // Kullanıcıyı oluşturuyoruz  // ilk nesne kullanicinin bilgilerini tutarken, ikinci nesne ise kullanıcının şifresini belirtiyor

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User"); // Kullanıcıyı "User" rolüne ekliyoruz
                    await _signInManager.SignInAsync(user, isPersistent: false); // Kullanıcıyı otomatik olarak giriş yapmış gibi işaretliyoruz // beni hatirla gibi bir ozellik ispersistent 
                    return RedirectToAction("Index", "Home"); // Kayıt başarılıysa anasayfaya yönlendiriyoruz
                }
                // yukarida bir hata olmus ise 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Bu email zaten kayıtlı."); // Email zaten kayıtlıysa hata mesajı ekliyoruz
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByEmailAsync(model.Email).Result; // Email'e göre kullanıcıyı buluyoruz
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false); // Şifre doğrulaması yapıyoruz  //lockoutOnFailure: false => başarısız giriş denemelerinde hesabın kilitlenmemesi için
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user); // Kullanıcının rollerini alıyoruz
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi."); // Giriş başarısızsa hata mesajı ekliyoruz
            return View(model);
        }

        [HttpPost]     
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [Route("userInfo")]
        public async Task<IActionResult> GetUserInfo()  // kullanici bilgilerini döndüren bir endpoint oluşturuyoruz  // bu endpoint'e sadece giriş yapmış kullanıcılar erişebilir
        {
            var user = await _userManager.GetUserAsync(User); // Şu anki kullanıcıyı alıyoruz

            if (user != null)
            {
                return Ok(new
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                });
            }
            return NotFound();
        }
    }
}