# 🌍 Travelio - Modern Seyahat ve Tur Rezervasyon Platformu

**Travelio**, kullanıcıların dünya genelindeki benzersiz turları keşfetmesini, detaylı tur takvimleri ve dinamik kontenjan yönetimi üzerinden rezervasyon yapmasını, yorum ve sorularla etkileşimde bulunmasını sağlayan; yöneticiler için ise gelişmiş MongoDB Aggregation, PDF/Excel raporlama ve kapsamlı bir **Admin Paneli** sunan uçtan uca bir seyahat ve tur yönetim platformudur.

---

## 🚀 Proje Hakkında & Öne Çıkan Özellikler

Proje, modern web standartlarına uygun olarak katmanlı mimari ve servis odaklı yaklaşımla geliştirilmiştir. Hem son kullanıcı (Public) hem de yönetim (Admin) tarafında yüksek performans ve zengin bir kullanıcı deneyimi (UI/UX) hedeflenmiştir.

* **Kapsamlı Admin Paneli:**
  * **Dashboard:** Anlık istatistikler, rezervasyon grafik analitiği, en popüler turlar, son yorumlar ve yanıt bekleyen sorular.
  * **Tur Yönetimi:** Çoklu görsel yükleme, galeri yönetimi, tarih bazlı kontenjan takibi ve aktif/pasif durum yönetimi.
  * **Kategori & Destinasyon Yönetimi:** Turları kategorize etme, şehir/ülke bazlı popüler rotalar belirleme.
  * **Rezervasyon & Finans Takibi:** Gelen rezervasyonları onaylama/iptal etme ve onaylanan toplam ciroyu izleme.
  * **Etkileşim Yönetimi:** Kullanıcılardan gelen yorumları denetleme ve turlar hakkındaki soruları yanıtlayabilme.
* **Zengin Kullanıcı Deneyimi (Public):**
  * **Gelişmiş Filtreleme & Arama:** Destinasyon, kategori, fiyat aralığı ve tarihe göre akıllı tur listeleme.
  * **Dinamik Tur Detayı:** Günlük tur akışı (Itinerary), tur öne çıkanları, çoklu fotoğraf galerisi ve dinamik tarih-kontenjan seçimi.
  * **Kullanıcı Paneli (`MyProfile`):** Geçmiş/gelecek rezervasyonları takip etme, favoriler, yorumlar ve kişisel bilgileri yönetme.
  * **Çok Dil Desteği:** ASP.NET Core Localization altyapısı ile çok dilli kullanım.

---

## 🛠️ Kullanılan Teknolojiler & Mimari

* **Backend:** ASP.NET Core MVC (.NET), LINQ, AutoMapper, FluentValidation
* **Database:** MongoDB (NoSQL Document Database - `IMongoCollection<T>` ve Nested Document yapıları)
* **Raporlama & Görselleştirme:** ClosedXML (Excel çıktıları), QuestPDF (PDF dokümanları), Chart.js (Grafik görselleştirme)
* **Dosya Yönetimi:** `IFormFile` tabanlı çoklu görsel işleme, `wwwroot` entegrasyonu ve `MemoryStream` üzerinden güvenli dosya akışları

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

---

## 📄 Gelişmiş Özellikler: PDF & Excel Raporlama Entegrasyonu

Projenin raporlama modülü, yöneticilerin belirli tur veya tur başlangıç tarihlerine göre katılımcı listelerini anlık olarak dışarı aktarabilmesini sağlar:
* **Excel Raporları:** *ClosedXML* kütüphanesi kullanılarak; müşteri adı, e-posta, telefon, tur adı, seçilen tarih, katılımcı sayısı, toplam tutar ve rezervasyon durumu gibi kritik veriler tablolaştırılır.
* **PDF Raporları:** *QuestPDF* altyapısı ile sunucu yorulmadan, diskte dosya bırakmaksızın doğrudan `MemoryStream` üzerinden hızlı ve şık tasarımlı çıktı alım mekanizması kurulmuştur.

---

## 📂 Proje Mimari Yapısı

```text
Travel.Web/
│
├── Areas/Admin/            # Yönetim Paneli Katmanı (Dashboard, Tour, Reservation vb.)
├── Controllers/            # Public Yüz Kontrolcüleri (Home, Tour, Profile vb.)
├── Services/               # İş Mantığı Servisleri (MongoDB, Raporlama, Dashboard, Capacity vb.)
├── DTOs/                   # Veri Transfer Nesneleri
├── Entities/               # NoSQL Belge Modelleri (Tour, Reservation, Comment vb.)
└── wwwroot/                # Statik Dosyalar ve Medya Depolama Alanı (/images/AllPicture/)