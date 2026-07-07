# Kapsamlı Restoran Sistemi Master Rehberi (Tek QR Kod & Dolu Masa Korumalı)

Bu doküman, sistemin uçtan uca çalışması için gereken Controller'ların yanı sıra **Hangi View (Arayüz) dosyalarının GÜNCELLENMESİ veya SIFIRDAN OLUŞTURULMASI gerektiğini ve içlerinde tam olarak ne olması gerektiğini** barındıran en detaylı rehberdir.

> [!WARNING]
> **Önemli Not:** Müşteri menüye girdiğinde sadece **BOŞ** masaları seçecek, ödeme yaptıktan sonra masa **DOLU** durumuna geçecek ve başka kimse o masaya sipariş veremeyecektir. Müşterinin tekrar sipariş verebilmesi için Admin veya Garson'un o masanın hesabını kapatıp masayı tekrar "Boş" statüsüne alması gerekmektedir.

---

## 0. Geriye Dönük Eklemeler (Eksik Kalan View ve Kodlar)

Bu bölüm, önceki aşamalarda (Aşama 1 ve 2) yazılmış olan Controller'ların tam olarak çalışabilmesi için **MUTLAKA GÜNCELLENMESİ (veya eksikse oluşturulması) gereken View (Arayüz)** dosyalarını ve ufak kod eklemelerini listeler. Lütfen mevcut projenizdeki dosyalarla karşılaştırarak ilerleyin.

### 0.1. Geriye Dönük View İhtiyaçları (Admin/TableController İçin)

`RestaurantApp.Web/Areas/Admin/Views/Table/` klasörü altındaki **mevcut dosyaları aşağıdaki kurallara göre GÜNCELLEMELİSİNİZ**:

1. **[GÜNCELLENECEK] `Index.cshtml`:**
   - **Mevcut Durum:** Hali hazırda projenizde bu dosya bulunmaktadır.
   - **Yapılacak Güncelleme:** Tüm masaların (Dolu/Boş) listelendiği ana sayfada, bir HTML `<table>` içinde `TableNumber` ve `Status` sütunlarının doğru gösterildiğinden emin olun.
   - **Yetki Farkı (Kritik):** Bu View'da `@if(User.IsInRole("Admin"))` bloğu kullanılarak "Yeni Masa Ekle", "Masayı Sil", "Hesabı Kapat" gibi yetki gerektiren eylem butonları **sadece Admin'e gösterilecek şekilde gizlenmelidir**. Sisteme giriş yapan **Garsonlar sadece "Detay" ve "Sipariş Gir" butonlarını görebilmelidir**. (Bu yetki kısıtlaması arayüzde kafa karışıklığını ve hatalı işlemleri önleyecektir).

2. **[GÜNCELLENECEK] `Create.cshtml`:**
   - **Mevcut Durum:** Hali hazırda projenizde bulunmaktadır.
   - **Yapılacak Güncelleme:** Mevcut sayfa eğer herkese açıksa, sadece Admin'in görebileceği şekilde (örneğin Layout menülerinden gizlenerek veya doğrudan yetki kontrolüyle) düzenlenmeli. İçinde `<input name="tableNumber">` ve `<button type="submit">Kaydet</button>` barındıran basit bir form standart UI/UX kurallarına uygun hale getirilmelidir.

3. **[GÜNCELLENECEK] `Details.cshtml`:**
   - **Mevcut Durum:** Hali hazırda projenizde bulunmaktadır.
   - **Yapılacak Güncelleme:** İlgili masaya ait verilmiş siparişlerin (`Order` ve `OrderItem` listesinin) gösterildiği, masanın adisyon / fiş sayfası olacak şekilde güncellenmelidir. Toplam tutarın dinamik olarak hesaplandığı ve Admin'in tıklayabileceği "Hesabı Kapat" butonunun bulunduğu alan mutlaka bu sayfada yer almalıdır.

### 0.2. Geriye Dönük Kod İhtiyaçları (Repository)

Müşteriye sadece "Boş" masaları listeleyebilmek için mevcut `TableRepository` dosyanıza şu sorguyu eklemelisiniz.

- **`ITableRepository.cs` (Interface) İçine Eklenecek:** 
  ```csharp
  Task<IEnumerable<Table>> GetEmptyTablesAsync();
  ```
- **`TableRepository.cs` (Sınıf) İçine Eklenecek:**
  ```csharp
  public async Task<IEnumerable<Table>> GetEmptyTablesAsync()
  {
      return await _context.Tables
          .Where(t => t.Status == RestaurantApp.Common.Enums.TableStatus.Empty)
          .ToListAsync();
  }
  ```

---

## AŞAMA 1: MÜŞTERİ KİMLİK DOĞRULAMA (ONBOARDING)

### 1.1. Google ile Hızlı Giriş (OAuth 2.0)

Müşterilerin hızlı sipariş verebilmesi için Google ile giriş altyapısı kurulmalıdır:

1. `dotnet add package Microsoft.AspNetCore.Authentication.Google` paketini kurun.
2. **appsettings.json:** `Authentication:Google:ClientId` ve `ClientSecret` tanımlamalarını ekleyin.
3. **Program.cs:** Servis yapılandırmasına `.AddGoogle(options => ...)` ekleyin.
4. **AccountController.cs (Backend):** `GoogleLogin` ve `GoogleResponse` Controller aksiyonlarını ekleyin.

> **[MEVCUT DOSYA GÜNCELLEMESİ] `RestaurantApp.Web/Views/Account/Login.cshtml`:**
> - **İçerik:** Standart E-Posta/Şifre giriş formunuzun altına Google ile giriş butonunu eklemelisiniz.
> ```html
> <!-- Login.cshtml içine eklenecek buton -->
> <hr class="mt-4 mb-4" />
> <a href="@Url.Action("GoogleLogin", "Account")" class="btn btn-outline-danger w-100 p-2 shadow-sm">
>     <i class="fa fa-google"></i> Google ile Giriş Yap
> </a>
> ```

### 1.2. E-posta ile Doğrulama (Manuel Kayıt)

1. `dotnet add package MailKit` paketini kurun.
2. **appsettings.json:** `EmailSettings` bloğu (SmtpServer, Port, Email, Password) oluşturun.
3. **IEmailService & EmailService:** SMTP mail gönderimi için bir servis sınıfı yazın ve DI (Dependency Injection) olarak ekleyin.
4. **AccountController (Register & ConfirmEmail):**
   - Kayıt olan kullanıcının `EmailConfirmed` değeri başlangıçta `false` yapılır.
   - Mail başarıyla gönderildikten sonra kullanıcı `EmailVerificationSent` sayfasına yönlendirilir.

> **[MEVCUT DOSYA KONTROLÜ] `RestaurantApp.Web/Views/Account/Register.cshtml`:**
> - **İçerik:** Ad, E-Posta ve Şifre alanlarının olduğu standart kayıt formu. (Mevcut projenizdeki bu dosyayı kontrol edin, eksikse doldurun).

> **[YENİ DOSYA OLUŞTURULACAK] `RestaurantApp.Web/Views/Account/EmailVerificationSent.cshtml`:**
> - **İçerik:** Kullanıcı kayıt olduktan sonra onu karşılayan "Lütfen e-postanızı kontrol edin" mesajı.
> ```html
> <div class="text-center mt-5">
>     <h2 class="text-success"><i class="fa fa-envelope"></i> E-Posta Gönderildi</h2>
>     <p>Giriş yapabilmek için e-posta adresinize gönderilen linke tıklayarak hesabınızı doğrulayın.</p>
>     <a href="@Url.Action("Login", "Account")" class="btn btn-primary mt-3">Giriş Yap</a>
> </div>
> ```

---

## AŞAMA 2: GARSON (WAITER) OPERASYONLARI

### 2.1. Admin TableController (Sadece Okuma Koruması)

Table işlemlerinde Garsonların yetkilerini kısıtlamak güvenlik için çok önemlidir:

```csharp
[Area("Admin")]
[Authorize(Roles = "Admin, Garson")] // Garsonlar sadece Index ve Details okuyabilir
public class TableController : Controller
{
    [HttpGet] public async Task<IActionResult> Index() { ... }
    [HttpGet] public async Task<IActionResult> Details(int id) { ... }

    // SADECE ADMIN İŞLEMLERİ (ÖNEMLİ):
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(string tableNumber) { ... }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> CloseTable(int id) { ... }
    
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) { ... }
}
```

### 2.2. Garson Sipariş Ekranı (WaiterController)

Garson "Sipariş Gir" butonuna bastığında açılacak, müşterinin sipariş vermesine gerek kalmadan garsonun adisyon açtığı sayfa.

```csharp
[Authorize(Roles = "Garson, Admin")]
public class WaiterController : Controller
{
    [HttpGet("Waiter/WaiterOrder")]
    public async Task<IActionResult> WaiterOrder(int tableId)
    {
        ViewBag.TableId = tableId;
        return View(await _productRepo.GetAllProductsAsync());
    }
}
```

> **[YENİ DOSYA OLUŞTURULACAK] `RestaurantApp.Web/Views/Waiter/WaiterOrder.cshtml`:**
> - **İçerik:** Garsonun masaya özel ürün seçtiği sayfa. Seçilen ürünlerin JavaScript ile frontend sepetine atıldığı ve `api/waiter/place-order` endpoint'ine JSON objesi olarak gönderildiği ekran.
> - **Önemli Detay:** Sipariş başarıyla veritabanına kaydedildikten sonra Javascript ile Garsonu `window.location.href = "/Admin/Table/Details/" + tableId;` koduyla masanın adisyon (Details) sayfasına geri döndürmelidir.

---

## AŞAMA 3: MÜŞTERİ SİPARİŞ AKIŞI (TEK MENÜ & DOLU MASA KORUMASI)

### 3.1. Genel Menü Ekranı (MenuController)

```csharp
public class MenuController : Controller
{
    private readonly IProductRepository _productRepo;
    private readonly ITableRepository _tableRepo;

    public MenuController(IProductRepository p, ITableRepository t) 
    { 
        _productRepo = p; 
        _tableRepo = t; 
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // View'a sadece BOŞ masaları yolluyoruz (Geriye dönük eklediğimiz metodu kullanıyoruz)
        ViewBag.EmptyTables = await _tableRepo.GetEmptyTablesAsync();
        return View(await _productRepo.GetAllProductsAsync());
    }
}
```

> **[YENİ DOSYA OLUŞTURULACAK] `RestaurantApp.Web/Views/Menu/Index.cshtml`:**
> - **İçerik:** Müşterinin web sitesine girdiğinde göreceği genel Menü ekranı. 
> - **Gerekli Alanlar:** 
>   1. Ürünlerin listelendiği kartlar (Resim, İsim, Fiyat) ve "Sepete Ekle" butonları.
>   2. Ekranın sağında veya altında "Sepetim" sepet içeriğinin özeti.
>   3. **Masa Seçimi (Kritik):** `<select>` elementi kullanılarak `ViewBag.EmptyTables` içerisinden sadece BOŞ masaların listelendiği bir Dropdown menü.
>   4. Kredi Kartı bilgilerinin girileceği form alanları.
>   5. "Ödeme Yap ve Siparişi Tamamla" butonu. (Bu buton tıklandığında içindeki JS kodu verileri toplayıp `api/customer/place-order` endpoint'ine atacak ve başarı durumunda sepeti boşaltacaktır).

### 3.2. Iyzico ve Sipariş API'si (CustomerOrderController)

Ödemeyi yönetmek ve siparişi finalize etmek için güvenli API Controller:
**Kurulum:** `dotnet add package iyzipay`

```csharp
[Route("api/customer")]
[ApiController]
public class CustomerOrderController : ControllerBase
{
    // Inject _payment, _order, _table servisleri buraya gelir...

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrderWithPayment([FromBody] PaymentDto req)
    {
        // 1. ÇİFTE KONTROL: MASA HALA BOŞ MU? (Aynı anda 2 kişi aynı masaya tıklamasın)
        var table = await _table.GetTableByIdAsync(req.TableId);
        if (table.Status != RestaurantApp.Common.Enums.TableStatus.Empty)
            return BadRequest(new { message = "Bu masa şu anda dolu! Başka bir masa seçin." });

        // 2. Iyzico Ödemesi (Senkronizasyon)
        var payRes = await _payment.ProcessPaymentAsync(new PaymentRequestDto { /* Kart bilgileri ve Sepet Tutarı */ });
        if(!payRes.IsSuccess) return BadRequest(new { message = "Ödeme Reddedildi! Lütfen kart bilgilerinizi kontrol edin." });
        
        // 3. Siparişi Kaydet ve MASAYI DOLU YAP
        var order = new Order { TableId = req.TableId, Status = OrderStatus.Preparing /* Sepet ürünleri ile maplenecek */ };
        await _order.CreateOrderAsync(order);
        
        // Müşteri başarılı şekilde ödediyse masa artık DOLU:
        await _table.UpdateTableStatusAsync(table.Id, RestaurantApp.Common.Enums.TableStatus.Occupied);
        
        return Ok(new { message = "Ödemeniz alındı, siparişiniz hazırlanıyor!" });
    }
}
```

---

## AŞAMA 4: GÜVENLİK (RATE LIMITING VE BLACKLIST)

### 4.1. Rate Limiting (.NET Built-in)
Spam siparişleri önlemek için yerleşik Rate Limiting kullanımı:
- **Program.cs** içerisine `builder.Services.AddRateLimiter(...)` eklenip global ve "OrderLimitPolicy" (örneğin 1 dakikada maks 3 sipariş) konfigürasyonu oluşturulacak.
- Sipariş metodunun (API) tepesine `[EnableRateLimiting("OrderLimitPolicy")]` eklenecek.

### 4.2. IP Blacklist (Kara Liste Middleware)
Kötü niyetli kullanıcıları sistemden engellemek için:
- `Middlewares/BlacklistMiddleware.cs` adında bir sınıf açılıp, veritabanından veya MemoryCache'den IP'lerin kontrol edildiği ve eşleşme varsa `403 Forbidden` veya `429 Too Many Requests` dönen bir ara katman (Middleware) yazılacak.
- **Program.cs**'de `app.UseMiddleware<BlacklistMiddleware>();` ile sisteme dahil edilecek.
