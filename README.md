# 🌍 Travelio - Modern Seyahat ve Tur Rezervasyon Platformu

**Travelio**, kullanıcıların dünya genelindeki turları keşfetmesini, tarih bazlı kontenjan yönetimiyle rezervasyon yapmasını, turları favorilere eklemesini, puan/yorum ve sorularla etkileşimde bulunmasını sağlayan; yöneticiler için ise **MongoDB Aggregation** tabanlı istatistikler, **PDF/Excel** katılımcı raporları ve kapsamlı bir **Admin Paneli** sunan uçtan uca bir seyahat ve tur yönetim platformudur.

Proje **Türkçe ve İngilizce** olarak tam çalışır; arayüz metinleri ASP.NET Core Localization ile, tur başlık/açıklamaları gibi içerikler ise MongoDB üzerinde çok dilli (`Tr` / `En`) olarak saklanır.

---

## 🔑 Hızlı Başlangıç

### Gereksinimler
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* MongoDB (yerel kurulum veya MongoDB Atlas)
* (Opsiyonel) MongoDB Compass – verileri görüntülemek için

### Kurulum
```bash
git clone https://github.com/Cihaansaahin/MyAcademy_MongoDbProject.git
cd MyAcademy_MongoDbProject/Travel.Web
dotnet restore
dotnet run
```

`appsettings.json` içindeki bağlantı bilgileri:

```json
"DatabaseSettings": {
  "ConnectionString": "mongodb://localhost:27017",
  "DataBaseName": "TravelioDb",
  "UserCollectionName": "Users",
  ...
},
"AdminUser": {
  "Email": "admin@travelio.com",
  "Password": "Admin123*"
}
```

Uygulama ilk açılışta `Users` koleksiyonunda **e-posta için unique index** oluşturur ve admin hesabı yoksa otomatik ekler.

### Demo Hesapları
| Rol | E-posta | Şifre |
| :--- | :--- | :--- |
| Admin | `admin@travelio.com` | `Admin123*` |
| Üye | Kayıt sayfasından yeni hesap oluşturabilirsiniz | – |

Admin paneli: `/Admin/Dashboard`

---

## 🚀 Öne Çıkan Özellikler

### 👤 Kullanıcı Tarafı (Public)
* **Ana Sayfa:** Admin panelinden yönetilen **banner slider**, en çok rezervasyon alan turlar (Aggregation), popüler destinasyonlar, gerçek toplam tur / destinasyon / üye sayıları ve son gerçek gezgin yorumları.
* **Gelişmiş Filtreleme:** Destinasyon, kategori, fiyat aralığı, süre, puan ve **tur tarihi + kişi sayısı** (kalan kontenjana göre) filtreleri.
* **Tur Detayı:** Fotoğraf galerisi, öne çıkan özellikler, accordion yapısında **gün gün tur programı**, tur tarihleri ve kalan kontenjan, **gerçek ortalama puan ve 1-5 yıldız dağılımı (%)**, onaylı sorular ve cevaplar.
* **Rezervasyon:** Tarih seçimi, yetişkin/çocuk sayısı, otomatik toplam ücret (çocuk %50). Kontenjan **atomik** olarak düşürülür; kontenjan biterse veya tarih geçmişse rezervasyon alınmaz.
* **Favoriler:** Liste, detay ve ana sayfadaki kalp butonu ile MongoDB `Favorites` koleksiyonuna kalıcı kayıt.
* **Profilim:** Rezervasyonlarım (durum rozetleriyle), Favorilerim, Yorumlarım (silme), Sorularım, Değerlendirmelerim (puan güncelleme) ve profil bilgisi güncelleme.
* **Çoklu Dil:** TR / EN arasında tek tıkla geçiş (cookie tabanlı).

### 🛠️ Admin Paneli
* **Dashboard:** Toplam/aktif/pasif tur, toplam rezervasyon ve kullanıcı, bekleyen sorular, bu ayki rezervasyonlar, toplam gelir; **son 7 gün / 30 gün / 6 ay** seçilebilir Chart.js grafiği; destinasyon ve kategori bazlı raporlar; en çok gelir getiren tur ve ortalama puan.
* **Tur Yönetimi:** TR/EN içerik, kapak ve çoklu galeri görseli yükleme, birden fazla tur tarihi ve **tarihe özel kontenjan**, gün gün program (şehir, ulaşım, yemek), aktif/pasif durum; ad, destinasyon, kategori ve duruma göre filtreleme.
* **Rezervasyon Yönetimi:** Onaylama/iptal, **tur, kullanıcı, tur tarihi aralığı ve duruma göre filtreleme**, tur + tarih seçerek rapor alma paneli.
* **Kategori, Destinasyon ve Banner Yönetimi:** CRUD işlemleri, dosyadan veya URL ile görsel yükleme.
* **Etkileşim Yönetimi:** Yorumları denetleme, cevaplanmamış soruları listeleme ve yanıtlama.

---

## 🧰 Kullanılan Teknolojiler & Mimari

| Alan | Teknoloji |
| :--- | :--- |
| Backend | ASP.NET Core MVC (.NET 10), LINQ, Primary Constructors |
| Veritabanı | MongoDB + MongoDB.Driver 3.x (Nested Document, Aggregation Pipeline, Unique Index) |
| Kimlik Doğrulama | Cookie Authentication + Claims, rol bazlı yetkilendirme (`Admin` / `Member`), `PasswordHasher` ile hash'lenmiş şifreler |
| Validasyon | FluentValidation (tur, tur tarihi, program günü, rezervasyon, yorum, soru, cevap, banner) – çok dilli hata mesajları |
| Eşleme | AutoMapper |
| Çoklu Dil | ASP.NET Core Localization (`IStringLocalizer`, `IHtmlLocalizer`, `SharedResource.en.resx`) + MongoDB'de `LocalizedString` (Tr/En) |
| Raporlama | ClosedXML (Excel), QuestPDF (PDF) – `MemoryStream` ile, sunucuda dosya tutulmadan |
| Grafik | Chart.js |
| Dosya Yönetimi | `IFormFile` ile görsel yükleme (uzantı ve 5 MB boyut kontrolü), `wwwroot/images` |

---

## 📊 MongoDB Aggregation Kullanımları

İstatistiklerin önemli bir kısmı `Find()` yerine **Aggregation Pipeline** ile hesaplanır:

| İstatistik | Kullanılan Aşamalar |
| :--- | :--- |
| En çok rezervasyon alan 5 tur | `$match` → `$group` → `$lookup` (Tours, kapak görseli) |
| En yüksek gelir sağlayan tur / toplam gelir | `$group` + `$sum` (`$toDecimal`) |
| Destinasyon bazında rezervasyon sayısı | `$lookup` (Reservations → Tours → Destinations) → `$group` → `$sort` |
| Kategori bazında tur sayısı | `$group` |
| Aylık ve günlük rezervasyon sayıları | `$match` → `$group` (`$dateToString`, Europe/Istanbul saat dilimi) |
| Ortalama puan ve 1-5 yıldız dağılımı | `$match` → `$group` |
| Ana sayfa "en çok tercih edilen turlar" | `$match` → `$group` |

---

## 📄 Tur Katılımcı Raporu (PDF & Excel)

Admin, tur listesinden veya Rezervasyonlar sayfasındaki **Katılımcı Raporu** panelinden rapor alabilir. Rapor **tüm tur** için veya **seçilen tur tarihi** için filtrelenebilir.

* **Alanlar:** Ad Soyad, E-posta, Telefon, Tur, Tur Tarihi, Yetişkin, Çocuk, Toplam Kişi, Rezervasyon Tarihi, Toplam Ücret, Durum
* **Excel (ClosedXML):** Başlık, filtre bilgisi, biçimlendirilmiş tarih/tutar sütunları ve iptaller hariç toplam satırı.
* **PDF (QuestPDF):** Yatay A4, satır renklendirme, toplam satırı ve sayfa numarası.
* Başlıklar ve durumlar seçili dile göre (TR/EN) üretilir; dosyalar `MemoryStream` üzerinden doğrudan indirilir.

---

## 🗄️ MongoDB Koleksiyonları

| Koleksiyon | Açıklama |
| :--- | :--- |
| `Tours` | Tur bilgileri; **nested** olarak tur tarihleri (kontenjan), gün gün program, galeri ve öne çıkan özellikler |
| `Destinations` | Şehir, ülke, görsel, popülerlik |
| `Categories` | Çok dilli kategori adları |
| `Reservations` | Kullanıcı, tur, seçilen tarih, yetişkin/çocuk, toplam ücret, durum (Bekliyor / Onaylandı / İptal Edildi) |
| `Users` | Ad, soyad, e-posta (unique), telefon, şifre hash'i, rol |
| `Comments` | Kullanıcı yorumu ve 1-5 puan |
| `Questions` | Kullanıcı soruları ve admin cevapları |
| `Favorites` | Kullanıcı – tur favori eşleşmeleri |
| `Banners` | Ana sayfa slider görselleri ve metinleri |

---

## 📸 Ekran Görüntüleri

### 📊 Admin Dashboard & Yönetim Panelleri
| Admin Dashboard | Tur Yönetimi |
| :---: | :---: |
| ![Admin Dashboard](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/AdminDashboard1.png) | ![Tour Management](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Tours.png) |

| Rezervasyon Yönetimi | Soru & Yorum Yönetimi |
| :---: | :---: |
| ![Reservations](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Rezervations.png) | ![Questions](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/QuestionManagement.png) |

| Kategori Yönetimi | Destinasyon Yönetimi |
| :---: | :---: |
| ![Categories](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Categories.png) | ![Destinations](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Destinations.png) |

### 🌐 Kullanıcı Arayüzü (Public)
| Ana Sayfa (Home) | Tur Detay & Rezervasyon |
| :---: | :---: |
| ![Home](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Home.png) | ![Tour Detail](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/TourDetay.png) |

| Kullanıcı Profili (`MyProfile`) | Yorum Yönetimi Ekranı |
| :---: | :---: |
| ![My Profile](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/MyProfile.png) | ![Review Management](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/ReviewManagement.png) |

### 🔐 Kimlik Doğrulama Sayfaları (Authentication)
| Giriş Yap (Sign In) | Hesap Oluştur (Create Account) |
| :---: | :---: |
| ![Sign In](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/Sign.png) | ![Create Account](https://raw.githubusercontent.com/Cihaansaahin/MyAcademy_MongoDbProject/master/Travel.Web/wwwroot/images/AllPicture/CreateAccount.png) |

> Ekran görüntülerini yenilemek için `wwwroot/images/AllPicture/` klasöründeki dosyaları **aynı adlarla** değiştirmeniz yeterlidir.

---

## 📂 Proje Mimari Yapısı

```text
Travel.Web/
│
├── Areas/Admin/            # Yönetim Paneli (Dashboard, Tour, Reservation, Banner vb.)
├── Controllers/            # Public kontrolcüler (Home, Tour, Account, Profile vb.)
├── DTOs/                   # Veri Transfer Nesneleri
├── Entities/               # MongoDB belge modelleri (Tour, Reservation, User vb.)
├── Helpers/                # Kimlik doğrulama ve görsel yükleme yardımcıları
├── Mappings/               # AutoMapper profilleri
├── Resources/              # Localization dosyaları (SharedResource.en.resx)
├── Services/               # İş mantığı (MongoDB, Dashboard/Aggregation, Rapor vb.)
├── Settings/               # Veritabanı ayarları
├── Validations/            # FluentValidation kuralları
└── wwwroot/                # Statik dosyalar ve yüklenen görseller (/images/)
```