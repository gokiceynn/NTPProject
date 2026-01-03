using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ListingMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Application.Services;

public class DatabaseBackupService
{
    private readonly AppDbContext _context;
    private readonly string _dbPath;
    
    public DatabaseBackupService(AppDbContext context)
    {
        _context = context;
        // SQLite database path comes from the configured EF connection
        _dbPath = _context.Database.GetDbConnection().DataSource;
    }
    
    /// <summary>
    /// Database'i belirtilen dizine yedekler
    /// </summary>
    /// <param name="backupDirectory">Yedek dizini (null ise default kullanılır)</param>
    /// <returns>Yedek dosyasının tam yolu</returns>
    public async Task<string> CreateBackupAsync(string? backupDirectory = null)
    {
        try
        {
            // Default backup directory
            if (string.IsNullOrWhiteSpace(backupDirectory))
            {
                backupDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "ListingMonitor_Backups"
                );
            }
            
            // Dizin yoksa oluştur
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
            
            // Yedek dosya adı (tarih damgalı)
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"listingmonitor_backup_{timestamp}.db";
            var backupPath = Path.Combine(backupDirectory, backupFileName);
            
            // Database dosyasının mevcut olduğunu kontrol et
            if (!File.Exists(_dbPath))
            {
                throw new FileNotFoundException($"Database dosyası bulunamadı: {_dbPath}");
            }
            
            // Database bağlantısını kapat (SQLite için gerekli)
            await _context.Database.CloseConnectionAsync();
            
            // Dosyayı kopyala
            File.Copy(_dbPath, backupPath, overwrite: true);
            
            // Bağlantıyı tekrar aç
            await _context.Database.OpenConnectionAsync();
            
            Console.WriteLine($"✅ Database yedeği oluşturuldu: {backupPath}");
            
            return backupPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Yedekleme hatası: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Sıkıştırılmış yedek oluşturur (.zip)
    /// </summary>
    public async Task<string> CreateCompressedBackupAsync(string? backupDirectory = null)
    {
        try
        {
            // Önce normal yedek oluştur
            var backupPath = await CreateBackupAsync(backupDirectory);
            
            // Sıkıştır
            var zipPath = backupPath + ".zip";
            
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zipArchive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath), CompressionLevel.Optimal);
            }
            
            // Sıkıştırılmamış yedeği sil
            File.Delete(backupPath);
            
            Console.WriteLine($"✅ Sıkıştırılmış yedek oluşturuldu: {zipPath}");
            
            return zipPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Sıkıştırılmış yedekleme hatası: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Yedekten geri yükler
    /// </summary>
    /// <param name="backupPath">Yedek dosyasının yolu</param>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Yedek dosyası bulunamadı: {backupPath}");
            }
            
            // Sıkıştırılmış mı kontrol et
            if (backupPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                // Geçici dizine çıkart
                var tempDir = Path.Combine(Path.GetTempPath(), "ListingMonitor_Restore");
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                Directory.CreateDirectory(tempDir);
                
                ZipFile.ExtractToDirectory(backupPath, tempDir);
                
                // Çıkartılan .db dosyasını bul
                var extractedDb = Directory.GetFiles(tempDir, "*.db").FirstOrDefault();
                if (extractedDb == null)
                {
                    throw new InvalidOperationException("Zip dosyasında database bulunamadı.");
                }
                
                backupPath = extractedDb;
            }
            
            // Mevcut database'i yedekle (güvenlik için)
            var safetyBackup = _dbPath + ".safety_backup";
            
            // Database bağlantısını kapat
            await _context.Database.CloseConnectionAsync();
            
            // Güvenlik yedeği al
            if (File.Exists(_dbPath))
            {
                File.Copy(_dbPath, safetyBackup, overwrite: true);
            }
            
            try
            {
                // Yedekten geri yükle
                File.Copy(backupPath, _dbPath, overwrite: true);
                
                // Bağlantıyı tekrar aç
                await _context.Database.OpenConnectionAsync();
                
                // Güvenlik yedeğini sil
                if (File.Exists(safetyBackup))
                {
                    File.Delete(safetyBackup);
                }
                
                Console.WriteLine($"✅ Database başarıyla geri yüklendi: {backupPath}");
            }
            catch
            {
                // Hata durumunda güvenlik yedeğinden geri al
                if (File.Exists(safetyBackup))
                {
                    File.Copy(safetyBackup, _dbPath, overwrite: true);
                    File.Delete(safetyBackup);
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Geri yükleme hatası: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Mevcut yedekleri listeler
    /// </summary>
    public List<BackupInfo> GetAvailableBackups(string? backupDirectory = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(backupDirectory))
            {
                backupDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "ListingMonitor_Backups"
                );
            }
            
            if (!Directory.Exists(backupDirectory))
            {
                return new List<BackupInfo>();
            }
            
            var backups = new List<BackupInfo>();
            
            // .db ve .zip dosyalarını bul
            var files = Directory.GetFiles(backupDirectory, "listingmonitor_backup_*.*")
                .Where(f => f.EndsWith(".db") || f.EndsWith(".zip"))
                .OrderByDescending(f => File.GetCreationTime(f));
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FileName = fileInfo.Name,
                    FullPath = file,
                    CreatedAt = fileInfo.CreationTime,
                    SizeBytes = fileInfo.Length,
                    IsCompressed = file.EndsWith(".zip")
                });
            }
            
            return backups;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Yedek listeleme hatası: {ex.Message}");
            return new List<BackupInfo>();
        }
    }
    
    /// <summary>
    /// Eski yedekleri temizler (belirli sayıda yedek tutar)
    /// </summary>
    public void CleanupOldBackups(int keepCount = 5, string? backupDirectory = null)
    {
        try
        {
            var backups = GetAvailableBackups(backupDirectory);
            
            if (backups.Count <= keepCount)
                return;
            
            // En eski yedekleri sil
            var toDelete = backups.Skip(keepCount);
            
            foreach (var backup in toDelete)
            {
                File.Delete(backup.FullPath);
                Console.WriteLine($"🗑️ Eski yedek silindi: {backup.FileName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Yedek temizleme hatası: {ex.Message}");
        }
    }
}

/// <summary>
/// Yedek dosyası bilgisi
/// </summary>
public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
    public bool IsCompressed { get; set; }
    
    public string SizeFormatted
    {
        get
        {
            if (SizeBytes < 1024)
                return $"{SizeBytes} B";
            if (SizeBytes < 1024 * 1024)
                return $"{SizeBytes / 1024.0:F1} KB";
            return $"{SizeBytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
