# 📝 Değişiklik Günlüğü (Changelog)

Tüm önemli değişiklikler bu dosyada belgelenir.

Format [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) standardına uygundur.

---

## [1.0.0] - 2025-01-03

### 🎉 İlk Sürüm

Tam özellikli ilk sürüm yayınlandı.

### ✨ Eklenen Özellikler

#### Site Yönetimi
- **AutoSupported Siteler**: Youthall, İlanburda, Microfon için hazır adapter'lar
- **Manuel Site Ekleme**: XPath/CSS seçicilerle özel site tanımlama
- **Site CRUD**: Ekleme, düzenleme, silme, aktif/pasif geçişi
- **Site Scraping**: Manuel ve otomatik ilan çekme

#### Alarm Kuralları
- **Anahtar Kelime Filtresi**: Regex word boundary ile tam kelime eşleştirme
- **Şehir Filtresi**: Büyük/küçük harf duyarsız
- **Fiyat Aralığı**: Min/Max fiyat filtresi
- **Site Filtresi**: Belirli siteye özel kurallar
- **Kural Testi**: Kaydetmeden önce eşleşen ilanları önizleme
- **Eşleşen İlan Görüntüleme**: Her kural için eşleşen ilanları listele

#### Email Bildirimleri
- **SMTP Entegrasyonu**: Gmail ve diğer SMTP sunucuları desteği
- **Test Email**: Bağlantı testi
- **Anlık Bildirim**: Kural eşleştiğinde otomatik email
- **Zamanlanmış Email**: 1/6/12/24 saat aralıklarla toplu gönderim
- **Manuel Gönderim**: Seçili siteden tüm ilanları gönder

#### UI/UX
- **Modern Tasarım**: Dark ve Light tema desteği
- **Tab Bazlı Navigasyon**: Dashboard, Siteler, Kurallar, İlanlar, Ayarlar, Loglar
- **Doğrudan Aksiyonlar**: Her öğenin yanında işlem butonları
- **Dinamik Filtreleme**: İlan listesinde site filtresi
- **Tema Kaydetme**: Seçilen tema veritabanına kaydedilir

#### Veri Yönetimi
- **SQLite Veritabanı**: Portable, zero-config
- **EF Core**: Code-first migrations
- **Backup/Restore**: Yedekleme ve geri yükleme
- **Otomatik Migration**: Uygulama başlangıcında şema güncelleme

#### Scraping Altyapısı
- **Adapter Pattern**: Her site için bağımsız adapter
- **ManualSiteScraper**: XPath/CSS seçicilerle genel parser
- **User-Agent Header**: Bot engeli aşma
- **HTML Parsing**: HtmlAgilityPack

---

## [0.9.0] - 2025-01-02

### 🔧 Beta Sürümü

#### Eklenen
- Kurallar sekmesi yeniden tasarlandı
- Her kural için doğrudan aksiyon butonları eklendi
- Eşleşen ilanlar paneli eklendi
- Anahtar kelime eşleştirmesi Regex ile güçlendirildi

#### Düzeltilen
- Kural popup başlığı "True" gösterme hatası
- Kural popup arka planı tema ile uyumsuzluk
- Kural kaydettikten sonra listede görünmeme
- "burs" aramasının "Bursa" ile eşleşmesi

---

## [0.8.0] - 2025-01-01

### 🔧 İlan Mail Gönderimi

#### Eklenen
- İlk çalıştırma mail gönderimi
- Seçili siteden ilanları gönderme
- Dinamik site seçimi (Tüm Siteler / Belirli Site)

#### Düzeltilen
- İlk çalıştırma mailinde sadece 320 ilan gönderilmesi
- NotificationLog nullable foreign key sorunu

---

## [0.7.0] - 2024-12-31

### 🔧 Siteler Sekmesi İyileştirmeleri

#### Eklenen
- Site satırlarında doğrudan aksiyon butonları (Düzenle, Scrape, Aktif/Pasif, Sil)
- Site tip ve kontrol aralığı badge'leri

#### Düzeltilen
- Header'daki scraping başlatmanın manuel siteleri içermemesi
- ScraperSchedulerService'in Manual siteleri atlaması

---

## [0.6.0] - 2024-12-30

### 🔧 Manuel Site Desteği

#### Eklenen
- Manuel site ekleme UI formu
- XPath ve CSS seçici desteği
- Site düzenleme ve silme fonksiyonları
- SITE_EKLEME_REHBERI.md dokümantasyonu

#### Düzeltilen
- SiteType ComboBox InvalidCastException
- SelectorType ComboBox InvalidCastException
- Manuel eklenen sitelerin ilan bulamaması
- URL Selector'da boşluk sorunu

---

## [0.5.0] - 2024-12-29

### 🔧 Tema Sistemi

#### Eklenen
- Dark/Light tema desteği
- Ayarlar sekmesinde tema toggle
- Tema seçiminin veritabanına kaydedilmesi

#### Kaldırılan
- Header'daki tema toggle butonu
- Tab underline (mavi çizgi)

---

## [0.4.0] - 2024-12-28

### 🔧 Adapter Güncellemeleri

#### Eklenen
- MicrofonAdapter: Next.js __NEXT_DATA__ JSON parsing
- IlanburdaAdapter: Table row parsing
- User-Agent header tüm HTTP isteklerine

#### Düzeltilen
- İlanburda 403 Forbidden hatası
- Microfon boş sonuç sorunu

---

## [0.3.0] - 2024-12-27

### 🔧 UI Formları

#### Eklenen
- SiteEditWindow: Site ekleme/düzenleme formu
- AlertRuleEditWindow: Kural ekleme/düzenleme formu
- SmtpSettingsWindow: SMTP ayarları formu

---

## [0.2.0] - 2024-12-26

### 🔧 Temel Servisler

#### Eklenen
- SiteService: Site CRUD
- AlertRuleService: Kural CRUD ve eşleştirme
- NotificationService: Email bildirimi
- ScraperSchedulerService: Zamanlanmış scraping
- ListingDiffService: Yeni ilan tespiti
- DatabaseBackupService: Yedekleme

---

## [0.1.0] - 2024-12-25

### 🎄 Proje Başlangıcı

#### Eklenen
- Clean Architecture proje yapısı
- Domain entities
- SQLite veritabanı
- Avalonia UI temel yapısı
- YouthallAdapter

---

## Sürüm Karşılaştırması

| Özellik | 0.1.0 | 0.5.0 | 1.0.0 |
|---------|-------|-------|-------|
| Desteklenen Siteler | 1 | 3 | 3+ Manuel |
| UI Tema | ❌ | ✅ Dark/Light | ✅ Gelişmiş |
| Kural Yönetimi | ❌ | ✅ Basit | ✅ Gelişmiş |
| Manuel Site | ❌ | ❌ | ✅ |
| Email | ❌ | ✅ | ✅ Gelişmiş |
| Backup | ❌ | ✅ | ✅ |

---

## Planlanan Özellikler

### v1.1.0 (Yakında)
- [ ] Playwright ile JS render desteği
- [ ] Proxy desteği
- [ ] Rate limiting konfigürasyonu
- [ ] Export to CSV/Excel

### v1.2.0 (Gelecek)
- [ ] Multi-language desteği
- [ ] Webhook bildirimleri
- [ ] REST API
- [ ] Docker desteği

---

## Katkıda Bulunanlar

- Proje sahibi ve geliştirici

---

<div align="center">

**Değişiklik önerileri için [GitHub Issues](https://github.com/gokiceynn/NTPProject/issues) kullanın 📝**

</div>
