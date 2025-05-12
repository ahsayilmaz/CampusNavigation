# Kampüs Navigasyon Sistemi

**Canlı Uygulama:** [campusnavigation.up.railway.app](http://campusnavigation.up.railway.app)

CampusNavigation, kullanıcıların bir üniversite kampüsündeki binalar arasında en kısa ve en verimli yolları bulmalarına yardımcı olmak için tasarlanmış bir web uygulamasıdır. Kullanıcıların başlangıç ve varış binalarını seçmelerine olanak tanıyan ve mesafeye veya gerçek zamanlı trafik koşullarına göre en uygun rota önerilerini sunan interaktif bir harita arayüzü sağlar.

## Temel Özellikler

-   **İnteraktif Harita Arayüzü**: Kampüs haritasını, binaları ve yolları gösteren interaktif bir şekilde görüntülemek için Leaflet.js kullanır.
-   **Bina Seçimi**: Kullanıcılar, açılır menülerden başlangıç ve bitiş binalarını kolayca seçebilirler.
-   **Çift Yönlü Rota Algoritması**:
    -   **En Kısa Yol**: Minimum fiziksel mesafeye sahip rotayı hesaplar.
    -   **Trafik Durumuna Göre Optimize Edilmiş Yol**: Daha hızlı alternatif rotalar önermek için kullanıcı tarafından bildirilen trafik/yoğunluk verilerini dikkate alır.
-   **Görsel Rota Gösterimi**: Hesaplanan rotayı harita üzerinde net bir şekilde vurgular.

## Teknolojiler

-   **Arka Uç (Backend)**: ASP.NET Core 6.0 - Güçlü ve ölçeklenebilir API geliştirme için.
-   **Veritabanı**: MySQL - Binalar, bina bağlantıları ve kullanıcı konumları hakkında bilgi depolar.
-   **Ön Yüz (Frontend)**: HTML, CSS, JavaScript.
-   **Haritalama Kütüphanesi**: Leaflet.js - İnteraktif harita oluşturma ve görselleştirme için.
-   **Dağıtım (Deployment)**: Docker & Railway.app - Tutarlı ortamlar ve kolay bulut dağıtımı için konteynerize edilmiştir.

## Mimari Genel Bakış

Uygulama, sorumlulukların ayrılması ve sürdürülebilirlik için katmanlı bir mimari kullanır:

-   **Sunum Katmanı (wwwroot)**: Statik ön yüz varlıklarını içerir (`index.html`, harita etkileşimi ve API çağrıları için JavaScript, stil için CSS).
-   **API Katmanı (Controllers)**:
    -   `CampusController.cs`: Kampüs verileriyle (binalar, bağlantılar) ilgili istekleri yönetir.
    -   `UserLocationController.cs`(uygulamaya entegre aşamasında/şuan kullanılamıyor): Kullanıcı konumu raporlamasını ve yoğunluk verilerini yönetir.
-   **Servis Katmanı (Services)**:
    -   `DatabaseService.cs`: Veritabanıyla etkileşim için temel iş mantığını uygular (binaları, bağlantıları getirme, yolları hesaplama - ancak yol hesaplama mevcut JS'de öncelikle istemci tarafındadır).
    -   `DatabaseSeedService.cs`: Uygulama başlangıcında veritabanını başlangıç verileriyle doldurur.
    -   `ShutdownCleanupService.cs`: Uygulama kapatıldığında temizleme görevlerini yönetir (dummy verileri temizlemek için oluşturuldu ancak şuanda kullanılmıyor).
-   **Veri Erişim Katmanı (Data & Models)**:
    -   `ApplicationDbContext.cs`: Veritabanı etkileşimleri için Entity Framework Core bağlamı.
    -   `Models`: Veri yapısını tanımlar (örneğin, `Building.cs`, `BuildingConnection.cs`, `UserLocation.cs`).
-   **Yapılandırma (Configuration)**: `appsettings.json` ve ortama özgü varyantlar, veritabanı bağlantı dizelerini ve diğer ayarları yönetir.

## Veritabanı Yapısı

MySQL veritabanı aşağıdaki ana tablolardan oluşur:

-   **Buildings**: Her binanın adını ve coğrafi koordinatlarını içeren ayrıntıları depolar.
    -   `Id` (Birincil Anahtar)
    -   `Name`
    -   `Latitude`
    -   `Longitude`
-   **BuildingConnections**: Binalar arasındaki doğrudan yolları (kenarları) ve mesafeyi tanımlar.
    -   `Id` (Birincil Anahtar)
    -   `FromBuildingId` (Buildings tablosuna Yabancı Anahtar)
    -   `ToBuildingId` (Buildings tablosuna Yabancı Anahtar)
    -   `Distance`
-   **UserLocations** (şuanda kullanılmıyor): Kampüs yollarında veya düğümlerinde kullanıcıların mevcut veya bilinen son konumunu izler (özellik aktifse ve kullanıcılar onaylarsa).
    -   `Id` (Birincil Anahtar)
    -   `UserId`
    -   `NodeName` (Bir düğümdeyse mevcut bina)
    -   `EdgeName` (Bir yoldaysa mevcut yol, örneğin, "BuildingA|BuildingB")
    -   `Timestamp`
-   **Users**: (şuanda kullanılmıyor)
    -   Şu anda, `UserLocations`'da anonim izleme için istemci tarafında bir `UserId` oluşturulur ve `localStorage`'da saklanır.

## Başlarken

### Önkoşullar

-   [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop) (Docker tabanlı kurulum için)
-   Bir MySQL örneği (Docker olmadan yerel olarak çalıştırılıyorsa veya geliştirme için)

### Seçenek 1: Docker ile Çalıştırma (Önerilen)

Uygulama, kolay kurulum ve dağıtım için Docker kullanılarak konteynerize edilmiştir.

1.  **Repoyu klonlayın:**
    ```bash
    git clone <repository-url>
    cd CampusNavigation
    ```
2.  **Docker Compose kullanarak uygulamayı derleyin ve çalıştırın:**
    Bu komut, .NET uygulama imajını oluşturacak ve hem uygulama konteynerini hem de bir MySQL veritabanı konteynerini (`db`) başlatacaktır.
    ```bash
    docker-compose up -d --build
    ```
3.  **Uygulamaya erişin:**
    Web tarayıcınızı açın ve [http://localhost:5000](http://localhost:5000) adresine gidin.

    `docker-compose.yml` dosyası servisleri tanımlar:
    -   `campusnavigation_app`: ASP.NET Core uygulaması.
    -   `db`: MySQL veritabanı servisi, gerekli ayarlarla ve veritabanı şemasını oluşturmak için bir başlatma betiğiyle (`mysql-init/01-schema.sql`) önceden yapılandırılmıştır.

### Seçenek 2: Yerel Olarak Çalıştırma (Geliştirme)

1.  **Depoyu klonlayın:**
    ```bash
    git clone <repository-url>
    cd CampusNavigation
    ```
2.  **MySQL'i yapılandırın:**
    -   Bir MySQL sunucusunun çalıştığından emin olun.
    -   Bir veritabanı oluşturun (örneğin, `CampusNavigation`).
    -   `src/CampusNavigation/appsettings.Development.json` dosyasındaki bağlantı dizesini güncelleyin:
        ```json
        {
          "ConnectionStrings": {
            "DefaultConnection": "Server=sizin_mysql_sunucunuz;Port=3306;Database=CampusNavigation;User=kullanıcı_adınız;Password=şifreniz;"
          }
          // ... diğer ayarlar
        }
        ```
    -   Tabloları ayarlamak için `mysql-init/01-schema.sql` betiğini veritabanınızda çalıştırın.
3.  **Uygulamayı çalıştırın:**
    ```bash
    cd src/CampusNavigation
    dotnet run
    ```
4.  **Uygulamaya erişin:**
    Genellikle `http://localhost:5000` veya `https://localhost:5001` adresinde mevcut olacaktır (`dotnet run` komutunun konsol çıktısını kontrol edin).

## Proje Yapısı

```
CampusNavigation/
├── CampusNavigation.sln        # Visual Studio Çözüm Dosyası
├── docker-compose.yml          # Docker Compose yapılandırması
├── Dockerfile                  # Uygulama için Dockerfile
├── README.md                   # Bu dosya
├── mysql-init/
│   └── 01-schema.sql           # Başlangıç veritabanı şeması
└── src/
    ├── CampusNavigation/           # Ana ASP.NET Core projesi
    │   ├── appsettings.json        # Uygulama ayarları
    │   ├── appsettings.Development.json # Geliştirmeye özel ayarlar
    │   ├── CampusNavigation.csproj # Proje dosyası (bağımlılıklar, derleme yapılandırması)
    │   ├── Program.cs              # Ana giriş noktası, servis yapılandırması
    │   ├── Controllers/            # API denetleyicileri
    │   │   ├── CampusController.cs
    │   │   └── UserLocationController.cs
    │   ├── Data/                   # Veritabanı bağlamı
    │   │   └── ApplicationDbContext.cs
    │   ├── Models/                 # Veri modelleri (varlıklar)
    │   │   ├── Building.cs
    │   │   ├── BuildingConnection.cs
    │   │   ├── DatabaseModels.cs   # (Potansiyel olarak birden fazla modeli veya bir temel sınıfı gruplar)
    │   │   ├── UserLocation.cs     #kullanılmıyor
    │   │   └── UserPresence.cs     #kullanılmıyor
    │   ├── Services/               # İş mantığı servisleri
    │   │   ├── DatabaseSeedService.cs
    │   │   ├── DatabaseService.cs
    │   │   ├── IDatabaseService.cs # DatabaseService için arayüz
    │   │   └── ShutdownCleanupService.cs   #kullanılmıyor
    │   └── wwwroot/                # Statik ön yüz varlıkları
    │       ├── index.html
    │       ├── css/site.css
    │       └── js/
    │           ├── algorithm.js      # Rota bulma algoritmaları (Dijkstra, vb.)
    │           ├── datas.js          # Eski veya yedek veriler
    │           ├── dataStructures.js # Özel veri yapıları (örneğin, algoritmalar için Kuyruk)
    │           ├── site.js           # Ana istemci tarafı mantığı, harita başlatma, olay yönetimi
    │           └── updateDistances.js # (Muhtemelen dinamik güncellemeler veya trafik ayarlamaları için)
    └── CampusNavigation.Tests/     # Birim/Entegrasyon testleri
        ├── ApiTests.cs
        └── CampusNavigation.Tests.csproj
```

## API Uç Noktaları (Endpoints)

Anahtar API uç noktaları şunları içerir:

-   `GET /api/Campus/buildings`: Tüm binaların bir listesini alır.
-   `GET /api/Campus/connections`: Binalar arasındaki bağlantıları (komşuluk listesi) alır.
-   `POST /api/UserLocation`: Kullanıcıların mevcut konumlarını (düğüm veya kenar) bildirmelerine olanak tanır.(kullanılmıyor)
-   `GET /api/UserLocation/density`: Kampüs yollarındaki kullanıcı yoğunluğu hakkında veri sağlar.(kullanılmıyor)
-   `POST /api/UserLocation/dummy-data`: (Geliştirme/Test) Örnek kullanıcı konumu verileri oluşturur.(kullanılmıyor)
-   `GET /api/diagnostics/dbstatus`: Veritabanı bağlantısı için sağlık kontrolü.

## Katkıda Bulunma

Katkılarınızı bekliyoruz! İyileştirmeler için önerileriniz varsa veya herhangi bir hata bulursanız, lütfen bir "issue" açın veya bir "pull request" gönderin.

1.  Repoyu forklayın.
2.  Özellik dalınızı oluşturun (`git checkout -b feature/HarikaOzellik`).
3.  Değişikliklerinizi commit edin (`git commit -m 'Harika Bir Özellik Ekle'`).
4.  Dala push edin (`git push origin feature/HarikaOzellik`).
5.  Bir Pull Request açın.

## Lisans

Bu proje MIT Lisansı altında lisanslanmıştır. Ayrıntılar için `LICENSE` dosyasına bakın (varsa, belirtilmemişse MIT olarak kabul edin).