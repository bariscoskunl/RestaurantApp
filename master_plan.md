# Restoran Sistemi Master Plan

## AŞAMA 1: HESAP KAPATMA & SATIŞ YANSITMA REVİZELERİ (KRİTİK)

### 1.1. Problem Analizi

Şu an `CloseTable` action'ı sadece siparişleri `Completed` yapıp masayı `Empty` konumuna getiriyor. Ancak:

> [!CAUTION]
> **Hesap kapatıldığında satış (`Sale`) tablosuna hiçbir kayıt atılmıyor!** Bu nedenle:
> - Günlük/Haftalık/Aylık raporlarda hiçbir veri görünmüyor
> - `ReportController` tamamen boş sonuç döndürüyor
> - Gelir takibi yapılamıyor

### 1.2. CloseTable Action'ına Satış Kaydı Eklenmesi

**[GÜNCELLENECEK]** `Areas/Admin/Controllers/TableController.cs` → `CloseTable` metodu

Mevcut `CloseTable` metodu aşağıdaki adımları yapmalıdır:

1. Masadaki tüm aktif siparişleri (`Preparing` veya `Served`) topla
2. Tüm `OrderItem`'ları birleştirerek bir `Sale` nesnesi oluştur
3. Her `OrderItem` için bir `SaleProducts` kaydı oluştur
4. `Sale` ve `SaleProducts` kayıtlarını veritabanına yaz
5. Siparişleri `Completed` olarak işaretle
6. Masayı `Empty` statüsüne çek

```csharp
[HttpPost, Authorize(Roles = "Admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CloseTable(int id)
{
    var table = await _tableRepository.GetTableByIdAsync(id);
    if (table == null) return NotFound();

    // 1. Aktif siparişleri al
    var orders = await _orderRepository.GetOrdersByTableIdAsync(id);
    var activeOrders = orders
        .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
        .ToList();

    if (activeOrders.Any())
    {
        // 2. Tüm sipariş kalemlerini topla
        var allItems = activeOrders.SelectMany(o => o.OrderItems).ToList();
        var totalPrice = allItems.Sum(oi => oi.UnitPrice * oi.Quantity);

        // 3. Sale kaydı oluştur
        var sale = new Sale
        {
            SaleDate = DateTime.Now,
            TotalPrice = totalPrice,
            TableNumber = int.TryParse(table.TableNumber, out var tn) ? tn : (int?)null
        };
        await _saleRepository.AddSaleAsync(sale);

        // 4. Her sipariş kalemi için SaleProducts kaydı oluştur
        foreach (var item in allItems)
        {
            var saleProduct = new SaleProducts
            {
                SaleId = sale.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductPrice = item.UnitPrice,
                ProductTotalPrice = item.UnitPrice * item.Quantity
            };
            await _saleRepository.AddSaleProductAsync(saleProduct);
        }

        // 5. Siparişleri Completed yap
        foreach (var order in activeOrders)
        {
            await _orderRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
        }
    }

    // 6. Masayı boşalt
    await _tableRepository.UpdateTableStatusAsync(id, TableStatus.Empty);

    return RedirectToAction(nameof(Index));
}
```

> [!IMPORTANT]
> **Dependency Injection:** `TableController`'a `ISaleRepository _saleRepository` inject edilmesi gerekmektedir. Constructor'a eklenmeli.

### 1.3. UpdateStatus Action'ındaki "Boş" Geçişi Düzeltmesi

**[GÜNCELLENECEK]** `TableController.cs` → `UpdateStatus` metodu

Şu an `UpdateStatus` ile masa "Boş"a alındığında da siparişler sadece `Completed` yapılıyor ama `Sale` kaydı oluşturulmuyor. Bu durum veri kaybına sebep olabilir.

**İki seçenek:**
- **Seçenek A (Önerilen):** "Boş"a alınırken de `CloseTable` ile aynı satış kaydı mantığı çalıştırılır (kod tekrarını önlemek için private bir metoda çıkarılır).
- **Seçenek B:** "Boş"a alma sadece `CloseTable` üzerinden yapılabilir, `UpdateStatus`'ta "Boş" seçeneği devre dışı bırakılır.

```csharp
// Ortak satış kaydı mantığı private metoda çıkarılır:
private async Task CreateSaleFromActiveOrders(int tableId)
{
    // CloseTable'daki satış kaydı mantığının aynısı buraya taşınır
    // Hem CloseTable hem UpdateStatus(Empty) bu metodu çağırır
}
```

### 1.4. "Hesap Bekliyor" (WaitingForBill) Durumu İyileştirmeleri

**[GÜNCELLENECEK]** `Table/Details.cshtml` & `TableController.cs`

Şu an "Hesap Bekliyor" durumu sadece görsel bir badge olarak duruyor, fonksiyonel bir işlevi yok. Aşağıdaki iyileştirmeler yapılmalıdır:

1. **Müşteriden Hesap Talebi:** Müşteri kendi `Home/Index` arayüzünden "Hesap İste" butonuna basabilmeli. Bu buton masanın durumunu `WaitingForBill` yapmalı.
   - `HomeController`'a bir `RequestBill` action eklenmeli
   - Müşterinin aktif siparişi olan masasını `WaitingForBill` statüsüne çekmeli

2. **Admin/Garson Bilgilendirme:** `Table/Index.cshtml`'de "Hesap Bekliyor" durumundaki masalar sarı renkte zaten görünüyor. Ek olarak:
   - "Hesap Bekliyor" masalarını üstte veya ayrı bir bölümde göstermek (öncelik sıralaması)
   - Opsiyonel: Sesli/görsel bildirim (tarayıcı notification)

3. **"Hesap Bekliyor" → Hesap Kapatma Akışı:** "Hesap Bekliyor" durumundaki bir masada "Hesabı Kapat" butonuna basıldığında, satış kaydı oluşturulmalı (1.2'deki mantık).

### 1.5. Sipariş Durumu Güncellemesi (Preparing → Served)

**[GÜNCELLENECEK]** `Table/Details.cshtml` & `TableController.cs`

Şu an sipariş durumu (`Preparing`, `Served`) değiştirme mekanizması Details sayfasında yok. Eklenmesi gereken:

1. Her siparişin yanında "Servis Edildi" butonu olmalı
2. Bu buton siparişin `Status`'unu `Preparing` → `Served` olarak güncellemeli
3. `TableController`'a `UpdateOrderStatus` action eklenmeli

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status, int tableId)
{
    await _orderRepository.UpdateOrderStatusAsync(orderId, status);
    return RedirectToAction(nameof(Details), new { id = tableId });
}
```

---

## AŞAMA 2: MÜŞTERİ SİPARİŞ AKIŞI İYİLEŞTİRMELERİ

### 2.1. Home/Index Müşteri Deneyimi

> [!NOTE]
> Müşterinin web sitesine girdiğinde gördüğü ekran `Home/Index`'tir. Ayrı bir `MenuController` veya genel menü ekranı **gerekli değildir**. Mevcut `Home/Index` bu işlevi zaten karşılamaktadır.

Mevcut `Home/Index` arayüzünde kontrol edilmesi gerekenler:

1. **Masa Seçimi:** Müşteri sipariş verirken boş masalardan seçim yapabiliyor mu? (JavaScript tarafında `GetEmptyTablesAsync` kullanılarak boş masalar dropdown'da listelenmeli)
2. **Sepet Özeti:** Sepet içeriğinin görsel olarak gösterilmesi (sidebar veya alt panel)
3. **"Hesap İste" Butonu:** Giriş yapmış ve aktif siparişi olan müşteri için görünür olmalı

### 2.2. Müşterinin Aktif Siparişlerini Görüntüleme

**[KONTROL EDİLECEK]** `HomeController.cs` → `GetTodaySales(userId)`

Bu endpoint mevcut ancak müşterinin **kendi aktif siparişlerini** (masasındaki tüm siparişleri) doğru gösterip göstermediği kontrol edilmeli. Arayüzde "Siparişlerim" bölümü düzgün çalışmalı.

---

## AŞAMA 3: GÜVENLİK (RATE LIMITING VE BLACKLIST)

### 3.1. Rate Limiting (.NET Built-in)

Spam siparişleri önlemek için yerleşik Rate Limiting kullanımı:

**[YENİ EKLENECEK]** `Program.cs`

```csharp
// Program.cs'e eklenecek servis yapılandırması:
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddFixedWindowLimiter("OrderLimitPolicy", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
});
```

**[GÜNCELLENECEK]** Sipariş endpoint'lerine `[EnableRateLimiting("OrderLimitPolicy")]` attribute'u eklenmeli:
- `HomeController.SaveOrder`
- `WaiterController.PlaceOrder`

**[GÜNCELLENECEK]** `Program.cs` pipeline'ına eklenmeli:
```csharp
app.UseRateLimiter(); // UseRouting'den sonra
```

### 3.2. IP Blacklist (Kara Liste Middleware)

Kötü niyetli kullanıcıları sistemden engellemek için:

**[YENİ OLUŞTURULACAK]** `Middlewares/BlacklistMiddleware.cs`

- Veritabanından veya `MemoryCache`'den IP adreslerini kontrol eden middleware
- Eşleşme varsa `403 Forbidden` döndürür
- Admin panelinden IP ekleme/çıkarma arayüzü (opsiyonel)

**[GÜNCELLENECEK]** `Program.cs`'de middleware'i aktif etme:
```csharp
app.UseMiddleware<BlacklistMiddleware>();
```

---

## AŞAMA 4: ÖDEME SİSTEMİ ENTEGRASYONU (Iyzico)

### 4.1. Iyzico Entegrasyonu

**[YENİ EKLENECEK]** Ödeme altyapısı

1. `dotnet add package iyzipay` paketini kurun
2. `appsettings.json`'a Iyzico API anahtarlarını ekleyin:
   ```json
   "Iyzico": {
     "ApiKey": "...",
     "SecretKey": "...",
     "BaseUrl": "https://sandbox-api.iyzipay.com"
   }
   ```
3. **IPaymentService & PaymentService:** Ödeme işlemi servisi yazılacak
4. DI'a `IPaymentService` kayıt edilecek

### 4.2. Müşteri Ödeme Akışı (CustomerOrderController)

**[YENİ OLUŞTURULACAK]** `Controllers/CustomerOrderController.cs` (API Controller)

```csharp
[Route("api/customer")]
[ApiController]
public class CustomerOrderController : ControllerBase
{
    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrderWithPayment([FromBody] PaymentDto req)
    {
        // 1. Masa hala boş mu? (Çifte kontrol)
        // 2. Iyzico ödeme işlemi
        // 3. Siparişi kaydet
        // 4. Masayı DOLU yap
        // 5. Sonuç dön
    }
}
```

> [!WARNING]
> Bu aşama, Iyzico'dan gerçek API anahtarları alındıktan sonra uygulanacaktır. Sandbox ortamında test edilmelidir.

---

## AŞAMA 5: QR KOD SİSTEMİ (EN SON YAPILACAK)

> [!IMPORTANT]
> QR Kod sistemi, tüm yukarıdaki aşamalar tamamlandıktan sonra **en son** aşamada eklenecektir. Diğer aşamalar stabil çalışmadan QR koda geçilmemelidir.

### 5.1. QR Kod Oluşturma (Her Masa İçin)

**Paket:** `dotnet add package QRCoder`

Her masanın kendine özel bir URL'i olacak ve bu URL bir QR koda dönüştürülecek.

1. **QR URL Formatı:** `https://domain.com/Home/Index?tableId={masaId}`
2. **Admin Panelinden QR Yazdırma:** `Table/Index.cshtml`'de her masa kartının yanında "QR Kodu İndir" butonu
3. **QR Kod Üretme Action'ı:**

```csharp
// TableController'a eklenecek
[HttpGet]
public IActionResult GenerateQrCode(int id)
{
    var url = $"{Request.Scheme}://{Request.Host}/Home/Index?tableId={id}";
    
    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrCodeData);
    var qrCodeBytes = qrCode.GetGraphic(20);
    
    return File(qrCodeBytes, "image/png", $"Masa_{id}_QR.png");
}
```

### 5.2. QR Kod ile Müşteri Akışı

Müşteri QR kodu okuttuğunda:
1. `Home/Index?tableId=X` sayfasına yönlendirilir
2. Masa otomatik olarak seçili gelir (dropdown'da o masa seçilmiş)
3. Müşteri giriş yapmamışsa → Giriş/Kayıt sayfasına yönlendirilir
4. Giriş yaptıktan sonra otomatik olarak seçili masa ile menü sayfasına döner

**[GÜNCELLENECEK]** `HomeController.Index` → `tableId` query parametresini kabul etmeli:
```csharp
public async Task<IActionResult> Index(int? tableId)
{
    // tableId varsa ViewBag.SelectedTableId = tableId ile view'a gönder
    // View'da masa dropdown'ı bu değerle önceden seçili gelsin
}
```

### 5.3. QR Kod Güvenlik Kuralları

- QR ile gelen müşteri **sadece o masaya** sipariş verebilmeli
- Masa zaten **dolu** ise müşteriye uyarı gösterilmeli
- Müşterinin aktif siparişi varsa, mevcut masasına yönlendirilmeli (başka masa seçememeli)

---

## ÖNCELİK SIRASI VE YÜRÜTME PLANI

| Sıra | Aşama | Öncelik | Tahmini Süre |
|------|-------|---------|-------------|
| 1️⃣ | **Hesap Kapatma & Satış Yansıtma** | 🔴 Kritik | Orta |
| 2️⃣ | **Müşteri Sipariş Akışı İyileştirmeleri** | 🟡 Önemli | Düşük |
| 3️⃣ | **Güvenlik (Rate Limiting & Blacklist)** | 🟡 Önemli | Orta |
| 4️⃣ | **Ödeme Sistemi (Iyzico)** | 🟠 Bağımlı | Yüksek |
| 5️⃣ | **QR Kod Sistemi** | 🔵 En Son | Orta |

> [!TIP]
> **Hemen başlanması gereken:** Aşama 1 (Hesap Kapatma & Satış Yansıtma). Bu olmadan raporlama sistemi tamamen işlevsiz kalmaktadır.
