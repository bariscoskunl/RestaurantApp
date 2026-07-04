# 🍽️ Lezzet Evi — Profesyonel Restoran & Adisyon Yönetim Sistemi

**Lezzet Evi**, **ASP.NET Core 8.0** tabanlı, modern bir restoranın tüm dijital operasyonlarını uçtan uca yönetmek için geliştirilmiş kapsamlı bir web uygulamasıdır. Proje; **N-Tier Architecture (Katmanlı Mimari)** ve **Repository Pattern** prensipleri üzerine inşa edilerek, yüksek sürdürülebilirlik ve kurumsal standartlarda kod kalitesi hedeflenmiştir.

Müşteri tarafında **akıllı sepet**, **masa bazlı sipariş** ve **dinamik menü**; yönetici tarafında ise **masa yönetimi (POS/Adisyon)**, **gelişmiş satış raporlaması** ve **tam yetkili CRUD** işlemlerini barındıran eksiksiz bir restoran otomasyon çözümüdür.

---

## 🚀 Öne Çıkan Özellikler

| Özellik | Açıklama |
| --- | --- |
| **🍽️ Masa Yönetimi (POS)** | Masaların **Boş / Dolu / Hesap Bekliyor** durumlarının renk kodlu kartlarla anlık takibi, masa bazlı hesap ve bekleme süresi kontrolü. |
| **🛒 Masaya Özel Akıllı Sepet** | Session tabanlı sepet; ürün ekleme/çıkarma/miktar güncelleme, masa seçimi ve aktif siparişe göre masa kilidleme mekanizması. |
| **📋 Detaylı Sipariş Takibi** | Her masadaki siparişlerin (Sipariş #1, #2, …) ayrı ayrı dökümü, ürün bazlı adet artırma/azaltma/silme, komple sipariş iptali ve hesap kapatma. |
| **📖 Dinamik Menü** | "Menüler" (Fix & Ekonomik) ve "Tekil Ürünler" sekmeleri ile kategorize edilmiş ürün katalogu, anlık kategori filtreleme. |
| **📊 Gelişmiş Raporlama** | Günlük, haftalık ve aylık satış verilerinin ürün bazlı analitik takibi. |
| **📤 Excel & PDF Export** | Satış raporlarını tek tıkla **EPPlus (Excel)** veya **iTextSharp (PDF)** formatında indirme. |
| **🛠️ Admin Paneli** | Ürün, kategori ve masa yönetimi için tam yetkili CRUD işlemleri; stok takibi ve kategori sıralama (SortOrder). |
| **🔐 Güvenlik & Identity** | Microsoft Identity ile e-posta/şifre ve Google OAuth ile güvenli giriş; Admin/User rol tabanlı yetkilendirme. |
| **🌙 Dark Mode** | Koyu/açık tema geçişi (localStorage ile kalıcı tercih). |
| **📱 Responsive Tasarım** | Tüm ekran boyutlarına uyumlu, mobil öncelikli modern arayüz. |

---

## 📸 Uygulama Ekran Görüntüleri

Uygulamanın tüm modüllerini ve işleyişini anlatan ekran görüntüleri aşağıda detaylı olarak sunulmuştur.

---

### 1️⃣ Ana Sayfa — Menüler ve Tekil Ürünler

Kullanıcıların ilk karşılaştığı dinamik menü sayfası. Üstte **"Menüler"** ve **"Tekil Ürünler"** sekmeleri ile iki ana görünüm sunulur. Menüler sekmesinde **Ekonomik Menüler** ve **Fix Menüler** başlıkları altında hazır paketler; Tekil Ürünler sekmesinde ise tüm bireysel ürünler kategorilere göre filtrelenebilir şekilde listelenir.

| Menüler Sekmesi (Ekonomik & Fix Menüler) | Tekil Ürünler Sekmesi (Kategori Filtreleme) |
| :---: | :---: |
| ![Menüler](screenshots/9.png) | ![Tekil Ürünler](screenshots/2.png) |

> **Menüler sekmesinde** Ekonomik Menüler (Öğrenci Döner Menü, Tako Sever Menü) ve Fix Menüler (Dolu Dolu Kutu Menü, Kahveli Keyif Menüsü) gibi hazır paketler görülmektedir.  
> **Tekil Ürünler sekmesinde** Dönerler, Takolar, Kutular ve Atıştırmalıklar, Soğuk İçecekler, Kahveler, Soslar gibi kategorilere göre filtreleme yapılabilir.

---

### 2️⃣ Akıllı Sepet Sistemi — Sipariş Verme Süreci

Kullanıcılar ürünleri sepete ekledikten sonra, sağ tarafta açılan **"My Cart"** panelinden sipariş sürecini yönetir. Sepet, **session tabanlı** çalışır ve her kullanıcı için bağımsız bir oturuma sahiptir. Masa seçimi yapıldıktan sonra aktif bir sipariş varsa masa değişikliği kilitlenir.

![Akıllı Sepet](screenshots/10.png)

> **Sepet durumunda** eklenen ürünler (ürün adı, fiyat, miktar), toplam tutar, **-/+** butonlarıyla miktar kontrolü ve masa seçimi ile birlikte gösterilir. Masa seçimi dropdown'u üzerinden kullanıcının hangi masada oturduğu belirlenir.

---

### 3️⃣ Tam Sayfa Görünüm — Ürünler ve Açık Sepet

Tekil Ürünler sayfasında gezinirken sepet panelinin sağdan açık haliyle birlikte tüm sayfanın genel görünümü. Ürün kartları, kategori filtreleri ve sepet paneli aynı anda görüntülenir.

![Tekil Ürünler ve Sepet Paneli](screenshots/11.png)

> Bu ekran görüntüsünde "Kutular ve Atıştırmalıklar" kategorisi seçili. Sepette **Tavuk Döner Dürüm** ve **Turşulu Tavuk Kutusu** yer alıyor. Toplam tutar **180,00 ₺** olarak hesaplanmış ve **Masa 2** seçili durumda. Siparişi tamamlamak için "Save Order" butonu kullanılır.

---

### 4️⃣ Sipariş Geçmişi Çekmecesi

Navbar üzerindeki **"Siparişler"** butonuna tıklandığında sağdan kayarak açılan çekmece (drawer). Kullanıcının **bugünkü siparişlerini** gösteren bu panel, her siparişin detaylarını listeler.

![Sipariş Çekmecesi](screenshots/3.png)

> Çekmecede **"Today's Orders"** başlığı altında her masanın geçmiş siparişleri tarih ve saat bazlı listelenir. Sipariş edilen ürünler, adetleri ve tutarları tablo formatında sunulur.

---

### 5️⃣ Masa Yönetimi — POS Panosu (Admin)

Restoran yöneticileri veya garsonlar için tasarlanmış **interaktif masa yönetim panosu**. Üst bölümde renk kodlu özet kartlar (🟢 Boş, 🔴 Dolu, 🟡 Hesap Bekliyor) bulunur. Her masa kartı, masanın durumunu, aktif sipariş bilgilerini (ürün sayısı, süre, toplam tutar) ve detay sayfasına erişim bağlantısını gösterir.

![Masa Yönetimi Panosu](screenshots/12.png)

> **Masa 2** "Dolu" statüsünde gösteriliyor. Kartın üzerinde **10 adet** ürün, **119 saat 17 dakika** bekleme süresi ve **1.210,00 ₺** toplam tutar bilgisi yer alıyor.  
> Boş masalarda (Masa 1, 3, 4, 5, 6, 8) yalnızca yeşil durum etiketi ve detay bağlantısı görünür. Sağ üstteki **"+ Yeni Masa Ekle"** butonu ile yeni masalar oluşturulabilir.

---

### 6️⃣ Masa Detay — Sipariş Takibi, İptal ve Hesap Kapatma (Admin)

Bir masaya tıklandığında açılan detay sayfası. Bu sayfa, masadaki tüm aktif siparişleri ayrı ayrı listeler. Her sipariş için **ürün adı**, **birim fiyat**, **adet** (artırma/azaltma butonları), **satır toplamı** ve **silme** butonu görünür. Sipariş başlığında **"Hazırlanıyor"** durumu, siparişin ne kadar süre önce verildiği ve **"İptal"** butonu yer alır.

![Masa Detay ve Siparişler](screenshots/13.png)

> **Masa 2** detayında iki ayrı sipariş görülüyor:
> - **Sipariş #1:** Tavuk Döner Dürüm (90,00 ₺) + Turşulu Tavuk Kutusu (90,00 ₺) — 1 dk önce verilmiş.
> - **Sipariş #2:** Tako Sever Menü (125,00 ₺) — 119 saat 28 dk önce verilmiş.
> 
> Altta **Genel Toplam: 305,00 ₺** ve **Toplam Ürün: 3 adet** özeti, ardından **"💳 Hesabı Kapat — 305,00 ₺"** butonu ile tek tuşla hesap kapatma imkanı sunulur.

---

### 7️⃣ Sipariş Detay Görünümü — Çoklu Siparişler (Admin)

Bir masada birden fazla sipariş olduğunda, her biri numarasıyla (Sipariş #6, #7, #8 gibi) ayrı ayrı listelenir. Bu görünüm, yoğun zamanlarda bile siparişlerin karışmamasını sağlar.

![Çoklu Sipariş Detayları](screenshots/16.png)

> **Sipariş #6** içerisinde Tako Sever Menü (125,00 ₺) bulunurken, **Sipariş #7** ve **#8** henüz içeriği olmayan (ürün eklenmemiş) veya iptal edilmiş siparişler olarak listelenir. Her sipariş başlığında "Hazırlanıyor" durumu ve zaman bilgisi yer alır.

---

### 8️⃣ Yeni Masa Ekleme (Admin)

Admin panelinden yeni masa oluşturmak için kullanılan form. Masa numarası olarak rakam, harf veya özel isimler (VIP-3 gibi) girilebilir.

![Yeni Masa Ekle](screenshots/17.png)

> Form, "Masa Numarası" alanı ve **"Masayı Kaydet"** butonu ile minimal ve kullanıcı dostu bir tasarıma sahiptir. Placeholder metin olarak *"Örn: 1, A1, VIP-3"* gösterilir.

---

### 9️⃣ Satış Raporları ve Veri Dışa Aktarma (Admin)

Günlük, haftalık ve aylık periyotlarla satış verilerinin analiz edildiği gelişmiş raporlama ekranı. Her satış kaydı, **tarih**, **toplam tutar** ve **satılan ürünlerin detayları** (adet, birim fiyat, ara toplam) ile birlikte gösterilir.

![Satış Raporları](screenshots/8.png)

> Haftalık rapor görünümünde her satışın detayı ürün bazında listelenir. Alt kısımda **"Raporları Dışa Aktar"** bölümü yer alır:
> - 🟢 **Excel Export:** Günlük Excel, Haftalık Excel, Aylık Excel
> - 🔴 **PDF Export:** Günlük PDF, Haftalık PDF, Aylık PDF
>
> Tek tıkla istenilen formatta rapor indirilir.

---

### 🔟 Admin Paneli — Ürün Yönetimi

Tüm ürünlerin listelendiği, düzenlendiği ve silindiği yönetim sayfası. Tablo üzerinde **ürün adı**, **fiyat**, **açıklama**, **içindekiler**, **kategori**, **görsel** ve **işlem butonları** (Düzenle/Sil) görünür. Üst kısımda **"Kategoriye Göre Filtrele"** dropdown'u ile hızlı filtreleme yapılabilir.

| Ürün Listesi (CRUD) | Ürün Düzenleme Formu |
| :---: | :---: |
| ![Ürün Listesi](screenshots/6.png) | ![Ürün Düzenleme](screenshots/7.png) |

> **Ürün Listesi:** Her ürün satırında küçük görsel (thumbnail), fiyat, açıklama ve kategori bilgisi ile birlikte listelenir. "Ürün Ekle" butonu ile yeni ürünler oluşturulabilir.
>
> **Ürün Düzenleme:** Ürün adı, fiyat, açıklama, içindekiler (pipe `|` ile ayrılmış), mevcut görsel önizleme, yeni görsel yükleme, stok durumu ve kategori seçimi gibi alanlar tek bir formda düzenlenebilir.

---

### 1️⃣1️⃣ Admin Paneli — Kategori Yönetimi

Ürünlerin gruplandığı kategorilerin yönetim sayfası. Her kategori için **ad**, **stok durumu**, **menü kategorisi mi?** bilgisi ve işlem butonları (Düzenle/Sil) görülür. Menü kategorisi olarak işaretlenen kategoriler (Ekonomik Menüler, Fix Menüler) ana sayfadaki "Menüler" sekmesinde yer alır.

| Kategori Listesi | Kategori Düzenleme Formu |
| :---: | :---: |
| ![Kategoriler](screenshots/14.png) | ![Kategori Düzenle](screenshots/15.png) |

> **Kategori Listesi:** Dönerler, Takolar, Kutular ve Atıştırmalıklar, Soğuk İçecekler, Kahveler, Soslar, Ekonomik Menüler, Fix Menüler, Burgerler gibi tüm kategoriler tablo halinde listelenir. "Menü Kategorisi Mi?" sütununda **Evet (Menü)** veya **Hayır** etiketi bulunur.
>
> **Kategori Düzenleme:** Kategori adı, stok durumu (Evet/Hayır) ve "Bu bir Menü Kategorisi mi?" seçimi yapılabilir. Eğer "Evet" seçilirse, bu kategori menü sekmesinde görünür.

---

### 1️⃣2️⃣ Kimlik Doğrulama — Giriş ve Kayıt Ekranları

Microsoft Identity altyapısı ile korunan, modern ve şık kullanıcı giriş/kayıt formları. Her iki ekranda da **Google ile giriş/kayıt** seçeneği mevcuttur.

| Giriş Yap (Login) | Kayıt Ol (Register) |
| :---: | :---: |
| ![Giriş Yap](screenshots/4.png) | ![Kayıt Ol](screenshots/5.png) |

> **Giriş Yap:** E-Posta ve Parola alanları, "Beni Hatırla" seçeneği, "Giriş Yap" butonu ve "Google ile Giriş Yap" alternatifi. Alt kısımda "Hesabınız yok mu? Kayıt Ol" bağlantısı.
>
> **Kayıt Ol:** E-Posta, Parola ve Parolayı Doğrula alanları, "Hesap Oluştur" butonu ve "Google ile Devam Et" alternatifi. Alt kısımda "Zaten bir hesabın var mı? Giriş Yap" bağlantısı.

---

### 📐 Katmanlı Mimari — Solution Explorer

Projenin Visual Studio üzerindeki katmanlı yapısını gösteren Solution Explorer görünümü.

![Katmanlı Mimari](screenshots/1.png)

---

## 🛠️ Teknik Mimari

Proje, **Separation of Concerns (Sorumlulukların Ayrılması)** ilkesine uygun olarak **4 temel katman** üzerine kurgulanmıştır:

| Katman | Proje | Sorumluluk |
| --- | --- | --- |
| **Sunum (Presentation)** | `RestaurantApp.Web` | MVC katmanı — Controller, View, ViewModel, ViewComponent ve Areas (Admin) yapılarını barındırır. |
| **İş Mantığı (Business)** | `RestaurantApp.Services` | Repository Pattern implementasyonları ve iş kurallarının yönetildiği katman. |
| **Veri Erişim (Data Access)** | `RestaurantApp.Data` | Entity Framework Core DbContext, Migration süreçleri ve veritabanı konfigürasyonları. |
| **Ortak (Common)** | `RestaurantApp.Common` | Entity sınıfları, Seed Data, DTO'lar ve paylaşılan yapılar. |

### Temel Mimari Prensipler

- **Repository Pattern:** Veri erişimi soyutlanarak, iş mantığı katmanından bağımsız hale getirilmiştir.
- **Dependency Injection:** Tüm bağımlılıklar `Program.cs` üzerinden IoC container'a kaydedilir.
- **Areas Yapısı:** Admin paneli, `/Areas/Admin/` altında ayrı Controller ve View'larla izole edilmiştir.
- **ViewComponent Mimarisi:** Sidebar bileşenleri (`SidebarViewComponent`, `AdminSidebarViewComponent`) modüler yapıda geliştirilmiştir.
- **Session Yönetimi:** Sepet verileri session üzerinde JSON formatında saklanır.

---

## 📦 Kullanılan Teknolojiler

| Teknoloji | Kullanım Alanı |
| --- | --- |
| **.NET 8.0 (ASP.NET Core MVC)** | Backend framework |
| **MS SQL Server & EF Core** | Veritabanı (Code First yaklaşımı) |
| **Microsoft Identity** | Kimlik doğrulama, yetkilendirme ve Google OAuth |
| **EPPlus** | Excel raporlama ve dışa aktarma |
| **iTextSharp** | PDF döküman üretimi |
| **Bootstrap 5** | Responsive UI framework |
| **Razor Views, CSS3, HTML5, JS** | Frontend geliştirme |

---

## 🔧 Kurulum ve Çalıştırma

> [!IMPORTANT]
> `appsettings.json` dosyası **güvenlik nedeniyle `.gitignore`'a eklenmiştir** ve Git'e gönderilmez. İçerdiği veritabanı bağlantısı, Google OAuth anahtarları ve SMTP şifresi gibi hassas bilgilerin kaynak kodunuza karışmaması için bu yaklaşım kullanılmıştır.

### Adım 1 — Projeyi Klonlayın
```bash
git clone https://github.com/bariscoskun441/RestaurantApp.git
```

### Adım 2 — Yapılandırma Dosyasını Oluşturun

Projeyi klonladıktan sonra `RestaurantApp.Web/appsettings.json` dosyası **mevcut olmayacaktır**. Bunun için hazırladığımız örnek şablonu kopyalayın:

```bash
# Windows (PowerShell)
Copy-Item "RestaurantApp.Web\appsettings.example.json" "RestaurantApp.Web\appsettings.json"
```

```bash
# Linux / macOS
cp RestaurantApp.Web/appsettings.example.json RestaurantApp.Web/appsettings.json
```

### Adım 3 — Kendi Bilgilerinizi Girin

Oluşturduğunuz `appsettings.json` dosyasını açın ve aşağıdaki alanları doldurun:

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=Be130_RestaurantApp;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Email": "YOUR_EMAIL@gmail.com",
    "Password": "YOUR_GMAIL_APP_PASSWORD",
    "SenderName": "RestaurantApp"
  }
}
```

| Alan | Açıklama |
|---|---|
| `ConnectionStrings.Default` | SQL Server bağlantı cümlesi. `Server=.` yerine kendi sunucu adınızı yazın. |
| `Authentication.Google.ClientId/Secret` | [Google Cloud Console](https://console.cloud.google.com/)'dan oluşturulan OAuth 2.0 kimlik bilgileri. Google ile giriş için zorunlu. |
| `EmailSettings.Password` | Gmail hesabı için oluşturulmuş **Uygulama Şifresi** (App Password). Normal Gmail şifreniz değil. |

> [!TIP]
> Google OAuth ve Gmail App Password oluşturmak istemiyorsanız, ilgili kod bloklarını yorum satırına alarak **yalnızca e-posta/şifre ile giriş** özelliğini kullanabilirsiniz.

### Adım 4 — Veritabanını Oluşturun

Package Manager Console üzerinden:
```bash
Update-Database
```

### Adım 5 — Projeyi Çalıştırın
```bash
dotnet run --project RestaurantApp.Web
```

> **Varsayılan Admin hesabı:** Seed Data ile birlikte gelen admin hesabı ile giriş yapabilirsiniz.

---

## 📫 İletişim

- **LinkedIn:** [linkedin.com/in/bariscoskun441](https://www.linkedin.com/in/bariscoskun441)
- **E-Posta:** bariscoskun441@gmail.com