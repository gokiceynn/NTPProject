# 📖 İlan Takip Sistemi - Kullanım Kılavuzu

<div align="center">

**Adım adım kullanım rehberi**

</div>

---

## 📋 İçindekiler

1. [İlk Kurulum](#1-i̇lk-kurulum)
2. [Dashboard Kullanımı](#2-dashboard-kullanımı)
3. [Site Yönetimi](#3-site-yönetimi)
4. [Alarm Kuralları](#4-alarm-kuralları)
5. [İlan Görüntüleme](#5-i̇lan-görüntüleme)
6. [Email Gönderimi](#6-email-gönderimi)
7. [Ayarlar](#7-ayarlar)
8. [İpuçları ve Püf Noktaları](#8-i̇puçları-ve-püf-noktaları)

---

## 1. İlk Kurulum

### 1.1 Uygulamayı Başlatma

```bash
cd /path/to/ListingMonitor
dotnet run --project src/ListingMonitor.UI
```

İlk başlatmada otomatik olarak:
- ✅ SQLite veritabanı oluşturulur
- ✅ Youthall ve İlanburda siteleri eklenir
- ✅ Varsayılan ayarlar yapılandırılır

### 1.2 SMTP Ayarlarını Yapılandırma (ÖNEMLİ!)

Email bildirimi almak için SMTP ayarları **zorunludur**.

**Adımlar:**
1. **Ayarlar** sekmesine git
2. **"⚙️ SMTP Ayarlarını Düzenle"** butonuna tıkla
3. Bilgileri doldur:

```
┌─────────────────────────────────────────┐
│ 📧 SMTP Ayarları                        │
├─────────────────────────────────────────┤
│ SMTP Host:      smtp.gmail.com          │
│ Port:           587                     │
│ TLS Kullan:     ✅                      │
│ Kullanıcı Adı:  your@gmail.com          │
│ Şifre:          xxxx-xxxx-xxxx-xxxx     │
│ Gönderen Email: your@gmail.com          │
├─────────────────────────────────────────┤
│    [🧪 Bağlantı Test Et]  [💾 Kaydet]   │
└─────────────────────────────────────────┘
```

#### Gmail için Özel Ayarlar:

1. Gmail hesabınızda **2 Adımlı Doğrulama** aktif edin
2. [Google Hesap Ayarları](https://myaccount.google.com/apppasswords) → Uygulama Şifreleri
3. "Mail" ve "Windows Bilgisayar" seçin → **Oluştur**
4. 16 haneli şifreyi SMTP şifre alanına yapıştırın

> ⚠️ Normal Gmail şifrenizi DEĞİL, uygulama şifresini kullanın!

### 1.3 İlk Test

1. SMTP ayarlarını kaydettikten sonra
2. **"🧪 Bağlantı Test Et"** butonuna tıkla
3. Başarılı mesajı görmelisiniz

---

## 2. Dashboard Kullanımı

### 2.1 Genel Bakış

Dashboard açıldığında 4 istatistik kartı görürsünüz:

```
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│    4     │  │   1105   │  │    3     │  │    24    │
│ Siteler  │  │ İlanlar  │  │ Kurallar │  │  Bugün   │
└──────────┘  └──────────┘  └──────────┘  └──────────┘
```

### 2.2 Scheduler Kontrolü

**Başlat/Durdur:**
```
Header → [▶️ Başlat] veya [⏸️ Durdur]
```

- **Başlat:** Tüm aktif siteleri belirtilen aralıklarla kontrol eder
- **Durdur:** Otomatik kontrolleri durdurur

> Varsayılan kontrol aralığı: 10 dakika

### 2.3 Aktivite Logları

Dashboard altında son aktiviteler görünür:
```
📈 Son Aktiviteler
├─ ✅ 10:30 - Youthall: 320 ilan çekildi
├─ ✅ 10:30 - İlanburda: 450 ilan çekildi
├─ 📧 10:31 - "Burs" kuralı için 2 bildirim gönderildi
└─ ⚠️ 10:32 - Eleman.net: Bağlantı hatası
```

---

## 3. Site Yönetimi

### 3.1 Site Listesi

**Siteler** sekmesinde tüm ekli siteler listelenir:

```
┌─────────────────────────────────────────────────────────┐
│ 🌐 Youthall                              ● Aktif       │
│    https://youthall.com/tr/talent-programs/            │
│    🏷️ AutoSupported  ⏱️ 10 dk                          │
│                                    [✏️][🔄][🔀][🗑️]   │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Site Ekleme

**Adımlar:**
1. **Siteler** sekmesi → **"➕ Site Ekle"** butonu
2. Site bilgilerini doldur
3. **"💾 Kaydet"** tıkla

#### Otomatik Desteklenen Site Ekleme:

```
Site Adı:       Youthall
Base URL:       https://youthall.com/tr/talent-programs/
Site Tipi:      AutoSupported   ← Önemli!
Aktif:          ✅
Kontrol Aralığı: 10 dakika
```

#### Manuel Site Ekleme (XPath):

```
Site Adı:       Eleman.net
Base URL:       https://www.eleman.net/is-ilanlari
Site Tipi:      Manual          ← Önemli!
Seçici Tipi:    XPath

Parser Ayarları:
┌─────────────────────────────────────────────────────────┐
│ Liste Seçici:    //div[contains(@class,'list-items')]   │
│                  /div[contains(@class,'list-item')]     │
│ Başlık Seçici:   .//h2[@class='title']/a                │
│ URL Seçici:      .//h2[@class='title']/a                │
│ ID Seçici:       .//h2[@class='title']/a/@href          │
│ Şirket Seçici:   .//span[@class='company-name']         │
│ Tarih Seçici:    .//span[@class='list-date']            │
│ Şehir Seçici:    .//span[@class='city-name']            │
└─────────────────────────────────────────────────────────┘
```

> 📖 Detaylı rehber için: [SITE_EKLEME_REHBERI.md](SITE_EKLEME_REHBERI.md)

### 3.3 Site İşlemleri

Her site satırının yanında 4 buton:

| Buton | Fonksiyon | Açıklama |
|-------|-----------|----------|
| ✏️ | Düzenle | Site ayarlarını değiştir |
| 🔄 | Scrape | Manuel olarak ilan çek |
| 🔀 | Aktif/Pasif | Siteyi aktif/pasif yap |
| 🗑️ | Sil | Siteyi ve ilanlarını sil |

### 3.4 Manuel Scraping

Anlık ilan çekmek için:
```
Site satırı → 🔄 butonu
```

Terminalde ilerlemeyi görebilirsiniz:
```
🔄 Youthall scraping başlatılıyor...
✅ Youthall scraping tamamlandı: 45 yeni ilan
```

---

## 4. Alarm Kuralları

### 4.1 Kural Listesi

**Kurallar** sekmesinde tüm alarm kuralları listelenir:

```
┌─────────────────────────────────────────────────────────┐
│ 🔔 Burs Bildirimleri                       ● Aktif     │
│    burs, scholarship, bursiyer                         │
│    📍 Tüm Siteler  🏙️ İstanbul  📧 email@test.com     │
│    ⏱️ 6h  🆕 Yeni                                      │
│                             [✏️][🎯][📧][🔄][🗑️]      │
└─────────────────────────────────────────────────────────┘
```

### 4.2 Kural Oluşturma

**Adımlar:**
1. **Kurallar** sekmesi → **"➕ Kural Ekle"** butonu
2. Kural bilgilerini doldur
3. **"🧪 Kuralı Test Et"** ile kontrol et
4. **"💾 Kaydet"** tıkla

#### Kural Formu:

```
┌─────────────────────────────────────────────────────────┐
│ 📋 Temel Bilgiler                                      │
├─────────────────────────────────────────────────────────┤
│ Kural Adı:       Burs Bildirimleri                     │
│ Hedef Site:      🌐 Tüm Siteler  ▼                     │
│ ✅ Aktif         ✅ Sadece Yeni İlanlar                │
├─────────────────────────────────────────────────────────┤
│ 🔍 Filtre Kriterleri                                   │
├─────────────────────────────────────────────────────────┤
│ Anahtar Kelimeler:                                     │
│ ┌─────────────────────────────────────────────────┐    │
│ │ burs, scholarship, bursiyer                      │    │
│ └─────────────────────────────────────────────────┘    │
│ 💡 Birden fazla kelime virgülle ayırın                 │
│                                                        │
│ Şehir:           İstanbul                              │
│ Min Fiyat:       0                                     │
│ Max Fiyat:       (boş = sınırsız)                      │
├─────────────────────────────────────────────────────────┤
│ 📧 Bildirim Ayarları                                   │
├─────────────────────────────────────────────────────────┤
│ Email Adresleri:                                       │
│ ┌─────────────────────────────────────────────────┐    │
│ │ email1@test.com, email2@test.com                │    │
│ └─────────────────────────────────────────────────┘    │
│                                                        │
│ ✅ Zamanlanmış Email Gönderimi Aktif                   │
│ Gönderim Aralığı: ⏱️ 6 Saat  ▼                         │
└─────────────────────────────────────────────────────────┘

        [🧪 Kuralı Test Et]      [İptal]  [💾 Kaydet]
```

### 4.3 Anahtar Kelime Filtreleme

**Tam Kelime Eşleştirmesi:**
- `burs` yazarsanız sadece "burs" kelimesini içeren ilanlar eşleşir
- `Bursa` şehrini içeren ilanlar **eşleşmez**

**Örnekler:**
| Anahtar Kelime | Eşleşen | Eşleşmeyen |
|----------------|---------|------------|
| `burs` | "Altuğ Fonu Bursu" | "Bursa İş İlanı" |
| `staj` | "Yaz Stajı 2025" | "Stajyer değil" |
| `remote` | "Remote Work" | "Remoteness" |

**Birden Fazla Kelime:**
```
burs, scholarship, bursiyer
```
Herhangi biri eşleşirse ilan seçilir (OR mantığı).

### 4.4 Kural Testi

Kuralı kaydetmeden önce test edin:

1. Filtreleri doldur
2. **"🧪 Kuralı Test Et"** tıkla
3. Sonuçları gör:

```
┌─ 🧪 Test Sonucu ──────────────────────────────────┐
│ ✅ Test Sonucu:                                   │
│ 📊 Toplam İlan: 1105                              │
│ 🎯 Eşleşen: 5                                     │
│ 🌐 Site: Tüm Siteler                              │
│ 🔍 Anahtar Kelimeler: burs, scholarship           │
│                                                   │
│ 📋 Örnek Eşleşmeler (ilk 5):                      │
│   • Altuğ Fonu Bursu - İstanbul...                │
│   • Koç Bursu 2025 - Türkiye geneli...            │
│   • Scholarship Program - Remote...               │
└───────────────────────────────────────────────────┘
```

### 4.5 Kural İşlemleri

Her kural satırının yanında 5 buton:

| Buton | Fonksiyon | Açıklama |
|-------|-----------|----------|
| ✏️ | Düzenle | Kural ayarlarını değiştir |
| 🎯 | Eşleşenler | Kurala uyan ilanları göster |
| 📧 | Test Mail | Eşleşen ilanları mail olarak gönder |
| 🔄 | Aktif/Pasif | Kuralı aktif/pasif yap |
| 🗑️ | Sil | Kuralı sil |

### 4.6 Eşleşen İlanları Görme

```
Kural satırı → 🎯 butonu
```

Panel açılır ve eşleşen ilanlar listelenir:

```
┌─ 🎯 Burs Bildirimleri ──── 5 ilan eşleşti ─────────┐
│                                                    │
│ ✅ Burs Bildirimleri analiz tamamlandı             │
│ 🌐 Site: Tüm Siteler                               │
│ 📊 Toplam 1105 ilan incelendi                      │
│ 🎯 5 ilan kurala uyuyor                            │
│                                                    │
│ ┌────────────────────────────────────────────────┐ │
│ │ Altuğ Fonu Bursu                    💰 0       │ │
│ │ 🌐 Youthall  📍 İstanbul  🏢 Altuğ Vakfı       │ │
│ └────────────────────────────────────────────────┘ │
│ ┌────────────────────────────────────────────────┐ │
│ │ Koç Bursu 2025                      💰 0       │ │
│ │ 🌐 Youthall  📍 Türkiye  🏢 Koç Holding        │ │
│ └────────────────────────────────────────────────┘ │
│                                                    │
│                        [📧 Test Mail Gönder]       │
└────────────────────────────────────────────────────┘
```

---

## 5. İlan Görüntüleme

### 5.1 İlan Listesi

**İlanlar** sekmesinde tüm ilanlar listelenir:

```
┌─────────────────────────────────────────────────────────┐
│ 📋 Toplam 1105 İlan                                    │
├─────────────────────────────────────────────────────────┤
│ 🔍 Filtre: [🌐 Tüm Siteler ▼]                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ┌─────────────────────────────────────────────────────┐│
│ │ Software Developer Intern                           ││
│ │ 🌐 Youthall  📍 İstanbul  🏢 ABC Tech   💰 15,000  ││
│ │ 📅 03.01.2025                                       ││
│ │                               [🔗 İlanı Görüntüle]  ││
│ └─────────────────────────────────────────────────────┘│
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 5.2 Site Filtresi

Dropdown'dan site seçerek filtreleyebilirsiniz:
- 🌐 Tüm Siteler
- 📍 Youthall
- 📍 İlanburda
- 📍 Eleman.net
- ...

### 5.3 İlan Detayı

Her ilan kartında:
- **Başlık:** İlan başlığı
- **Site:** Hangi siteden geldiği
- **Şehir:** İlan lokasyonu
- **Şirket:** İlan veren şirket
- **Fiyat:** Maaş/Ücret (varsa)
- **Tarih:** İlk görülme tarihi
- **Link:** Orijinal ilana git

---

## 6. Email Gönderimi

### 6.1 Manuel İlan Gönderimi

**Ayarlar** sekmesinde **"İlan Mail Gönder"** bölümü:

```
┌─────────────────────────────────────────────────────────┐
│ 📤 İlan Mail Gönder                                    │
├─────────────────────────────────────────────────────────┤
│ Site Seçimi:    [📊 Tüm Siteler ▼]                     │
│ Alıcı Email:    your@email.com                         │
│                                                         │
│ [📧 İlanları Mail Gönder]   ✅ Mail gönderildi!        │
└─────────────────────────────────────────────────────────┘
```

**Seçenekler:**
- **📊 Tüm Siteler:** Tüm sitelerden tüm ilanları gönder
- **🌐 Youthall:** Sadece Youthall ilanlarını gönder
- **🌐 İlanburda:** Sadece İlanburda ilanlarını gönder

### 6.2 Kural Bazlı Test Mail

Kurala uyan ilanları mail olarak göndermek için:

```
Kurallar → [Kural satırı] → 📧 butonu
```

veya

```
Kurallar → [Kural satırı] → 🎯 butonu → [📧 Test Mail Gönder]
```

### 6.3 Zamanlanmış Email

Kural oluştururken **"Zamanlanmış Email"** aktif edilirse:
- Belirtilen aralıklarla (1/6/12/24 saat) otomatik email gönderilir
- Sadece yeni ilanlar gönderilir
- Toplu email formatında

---

## 7. Ayarlar

### 7.1 Tema Ayarları

```
🎨 Tema Ayarları
├─ 🌙 Gece Modu [Toggle] ☀️ Gündüz Modu
```

- **Gece Modu:** Koyu arka plan (#0F172A)
- **Gündüz Modu:** Açık gri arka plan (#F1F5F9)

### 7.2 SMTP Ayarları

```
📧 SMTP Ayarları
└─ [⚙️ SMTP Ayarlarını Düzenle]
```

### 7.3 Kontrol Aralığı

```
⚡ Hızlı Ayarlar
├─ Kontrol Aralığı (dakika): [10]
└─ [📧 Test Mail Gönder]
```

### 7.4 Veritabanı Yönetimi

```
🗃️ Veritabanı Yönetimi
├─ [💾 Yedekle]
├─ [📂 Yedeklerden Geri Yükle]
├─ [🧹 Eski Yedekleri Temizle]
└─ [⚠️ İlan Verilerini Sıfırla]
```

**Yedekleme:**
- Manuel yedek oluşturur
- `backups/` klasörüne kaydeder
- Tarih damgalı dosya adı

**Geri Yükleme:**
- Listeden yedek seç
- **"📂 Geri Yükle"** tıkla
- Uygulama yeniden başlar

---

## 8. İpuçları ve Püf Noktaları

### 8.1 Etkili Anahtar Kelimeler

✅ **İyi Örnekler:**
```
burs, scholarship           → Burs ilanları
staj, intern, internship    → Staj ilanları
junior, entry level         → Giriş seviye pozisyonlar
remote, uzaktan             → Uzaktan çalışma
```

❌ **Kaçınılması Gerekenler:**
```
a, bir, the                 → Çok genel
developer                   → Çok geniş sonuç
```

### 8.2 Site Ekleme İpuçları

1. **Önce tarayıcıda test edin:**
   - F12 → Elements ile DOM yapısını inceleyin
   - İlan listesinin container'ını bulun
   - Her ilanın ortak sınıf/id'sini belirleyin

2. **XPath yazarken:**
   ```xpath
   // → Döküman genelinde ara
   . → Mevcut node'dan başla (önemli!)
   //div[@class='item'] → class="item" olan div
   //a[contains(@href,'job')] → href'inde "job" geçen linkler
   ```

3. **Test edin:**
   - Site ekle, manuel scrape yap
   - Terminalde hata kontrolü
   - İlanlar sekmesinde sonuçları kontrol et

### 8.3 Performans İpuçları

1. **Kontrol aralığını optimize edin:**
   - Yoğun siteler: 5-10 dakika
   - Az güncellenen siteler: 30-60 dakika

2. **Kullanmadığınız siteleri pasif yapın:**
   - Site satırı → 🔀 butonu

3. **Eski yedekleri temizleyin:**
   - Ayarlar → 🧹 Eski Yedekleri Temizle

### 8.4 Sorun Giderme

**İlan bulunamıyor:**
```
1. Site URL'ini kontrol et
2. XPath seçicilerini kontrol et
3. Terminalde hata mesajlarını oku
4. Tarayıcıda siteyi aç, yapı değişmiş olabilir
```

**Email gitmiyor:**
```
1. SMTP ayarlarını kontrol et
2. Gmail ise uygulama şifresini kullan
3. "Bağlantı Test Et" ile test et
4. Spam klasörünü kontrol et
```

**Uygulama açılmıyor:**
```
1. .NET 8.0 SDK kurulu mu?
2. dotnet --version ile kontrol et
3. Temiz build al:
   rm -rf src/*/bin src/*/obj
   dotnet build src/ListingMonitor.UI
```

---

## 📞 Yardım

- **GitHub**: [github.com/gokiceynn/NTPProject](https://github.com/gokiceynn/NTPProject)
- **GitHub Issues:** [Bug raporları ve özellik istekleri](https://github.com/gokiceynn/NTPProject/issues)
- **Dokümantasyon:** README.md, SITE_EKLEME_REHBERI.md
- **Log Takibi:** Terminal çıktısını kontrol edin

---

<div align="center">

**İyi kullanımlar! 🎯**

</div>
