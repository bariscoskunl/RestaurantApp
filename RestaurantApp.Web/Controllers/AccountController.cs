using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Common.Models;
using RestaurantApp.Services.Interfaces;
using RestaurantApp.Web.Models;
using System.Security.Claims;

namespace RestaurantApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(); // Role manager'ı alıyoruz
            _userManager = userManager;  // User manager'ı alıyoruz
            _signInManager = signInManager;  // Sign-in manager'ı alıyoruz     // Bu DI islemlerini Identity framework'ün sağladığı servisler üzerinden yapıyoruz  (Paket dahilinde)
            _emailService = emailService;
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _userManager.FindByEmailAsync(model.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User"); // Kullanıcıyı "User" rolüne ekliyoruz

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, token = token },
                        Request.Scheme
                        );
                    var emailBody = $@"
                    <h2>E-posta Doğrulama</h2>
                    <p>Merhaba,</p>
                    <p>Hesabınızı doğrulamak için aşağıdaki bağlantıya tıklayın:</p>
                    <a href='{confirmationLink}'>Hesabımı Doğrula</a>
                    <p>Bu bağlantı 24 saat geçerlidir.</p>";

                    await _emailService.SendEmailAsync(model.Email, "E-posta Doğrulama - RestaurantApp", emailBody);

                    TempData["RegistrationComplete"] = true;
                    return RedirectToAction("EmailVerificationSent"); // Kayıt başarılıysa kayit sayfasina yönlendiriyoruz
                }
                // yukarida bir hata olmus ise 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                // Eğer e-posta zaten kullanımadaysa hatayı ekrana bas
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda. Lütfen başka bir e-posta adresi deneyin veya giriş yapın.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "E-posta adresiniz başarıyla doğrulandı ve giriş yapıldı.";
                return RedirectToAction("Index", "Home");
            }
            TempData["ErrorMessage"] = "Doğrulama bağlantısı geçersiz veya süresi dolmuş.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EmailVerificationSent()
        {
            if (TempData["RegistrationComplete"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
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
                var user = await _userManager.FindByEmailAsync(model.Email); // Email'e göre kullanıcıyı buluyoruz
                if (user != null)
                {  
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty,
                           "E-posta adresiniz henüz doğrulanmamış. Lütfen e-postanızı kontrol edin.");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false); // Şifre doğrulaması yapıyoruz  //lockoutOnFailure: false => başarısız giriş denemelerinde hesabın kilitlenmemesi için                     
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user); // Kullanıcının rollerini alıyoruz
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        }
                        return RedirectToAction("Index", "Home"); // User rolündeki kullanıcıları ana sayfaya yönlendiriyoruz
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi."); // Giriş başarısızsa hata mesajı ekliyoruz
            return View(model);
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return RedirectToAction("Login");
                }

                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(user, "User");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            return RedirectToAction("Index", "Home");
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