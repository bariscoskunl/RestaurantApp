# 🍔 FrejSnack - Restoran Yönetim Sistemi

FrejSnack, **ASP.NET Core 8.0** tabanlı, modern bir restoranın tüm dijital operasyonlarını uçtan uca yönetmek için geliştirilmiş kapsamlı bir web uygulamasıdır. Proje; **N-Tier Architecture (Katmanlı Mimari)** ve **Repository Pattern** prensipleri üzerine inşa edilerek, yüksek sürdürülebilirlik ve kurumsal standartlarda kod kalitesi hedeflenmiştir.

## 🚀 Öne Çıkan Özellikler

- **Dinamik Menü Yönetimi:** Ürünlerin kategorilere göre listelenmesi ve anlık filtreleme desteği.
- **Gelişmiş Sepet Sistemi:** Session tabanlı; ürün ekleme, çıkarma ve miktar güncelleme özellikli akıllı sepet.
- **Admin Kontrol Paneli:** Ürün, kategori ve sipariş yönetimi için tam yetkili (CRUD) yönetim alanı.
- **Gelişmiş Raporlama:** Günlük, haftalık ve aylık satış verilerinin analitik takibi.
- **Profesyonel Export Seçenekleri:** Raporları tek tıkla **PDF (iTextSharp)** ve **Excel (EPPlus)** formatında dışa aktarma.
- **Güvenlik ve Identity:** Microsoft Identity ile güvenli kayıt, login ve Role-Based (Admin/User) yetkilendirme.

---

## 📸 Uygulama Ekran Görüntüleri

Tüm sistemin işleyişini gösteren ekran görüntüleri aşağıda gruplandırılmıştır:

### 🏠 Kullanıcı Arayüzü & Menü
Modern ve responsive tasarıma sahip dinamik ürün katalogu.
![Ana Menü](screenshots/9.png)

### 🛒 Sipariş ve Sepet Süreci
Sepet yönetiminden sipariş onayına kadar kesintisiz müşteri deneyimi.
| Alışveriş Sepeti | Sipariş Özeti |
| :---: | :---: |
| ![Sepet](screenshots/10.png) | ![Sipariş Onayı](screenshots/11.png) |

### 🔐 Kimlik Doğrulama & Profil Yönetimi
Müşteri ve yöneticiler için güvenli giriş ve kayıt ekranları.
| Giriş Ekranı | Kayıt Formu Detayı |
| :---: | :---: |
| ![Giriş](screenshots/4.png) | ![Kayıt](screenshots/5.png) |

### 📊 Yönetici (Admin) Paneli & Raporlama
İşletme sahipleri için kapsamlı yönetim ve veri dışa aktarma araçları.
| Ürün Listesi ve CRUD | Ürün Ekleme/Düzenleme | Satış Raporları |
| :---: | :---: | :---: |
| ![Ürün Yönetimi](screenshots/6.png) | ![Ürün Formu](screenshots/7.png) | ![Raporlama](screenshots/8.png) |

---

## 🛠️ Teknik Mimari

Proje, **Separation of Concerns (Sorumlulukların Ayrılması)** ilkesine uygun olarak 4 temel katman üzerine kurgulanmıştır:

1. **RestaurantApp.Web:** UI (MVC) katmanı. Controller, View ve ViewModel yapılarını barındırır. Admin süreçleri için **Areas** yapısı kullanılmıştır.
2. **RestaurantApp.Services:** İş mantığının (Business Logic) yürüdüğü ve **Repository Pattern** implementasyonlarının bulunduğu katman.
3. **RestaurantApp.Data:** Veri erişim katmanı. Entity Framework Core, DbContext ve Migration süreçlerini yönetir.
4. **RestaurantApp.Common:** Entity'ler, Seed Data ve DTO yapılarını içeren ortak katman.

### Katmanlı Mimari (Solution Explorer)
![Katmanlı Mimari](screenshots/1.png)

---

## 📦 Kullanılan Teknolojiler

- **Backend:** .NET 8.0 (ASP.NET Core MVC)
- **Database:** MS SQL Server & EF Core (Code First)
- **Identity:** Microsoft Identity Framework
- **Reporting:** - **EPPlus:** Excel raporlama çözümleri.
  - **iTextSharp:** PDF döküman üretimi.
- **Frontend:** Bootstrap 5, Razor Views, CSS3, HTML5

---

## 🔧 Kurulum ve Çalıştırma

1. **Projeyi klonlayın:**
   ```bash
   git clone [https://github.com/bariscoskun441/RestaurantApp.git](https://github.com/bariscoskun441/RestaurantApp.git)

2.  `appsettings.json` dosyasındaki `ConnectionStrings` bölümünü kendi SQL Server bilgilerinize göre güncelleyin.

3.  Package Manager Console üzerinden veritabanını oluşturun:
    ```bash
    Update-Database
    ```
4.  Projeyi çalıştırın.

📫 İletişim
LinkedIn:: www.linkedin.com/in/bariscoskun441
E-Posta:: bariscoskun441@gmail.com