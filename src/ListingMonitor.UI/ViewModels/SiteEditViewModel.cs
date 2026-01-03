using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Application.Services;

namespace ListingMonitor.UI.ViewModels;

public partial class SiteEditViewModel : ObservableObject
{
    private readonly SiteService _siteService;
    private readonly Action _onClose;
    
    [ObservableProperty] private Site? _site;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _baseUrl = "";
    [ObservableProperty] private int _siteTypeIndex = 0; // 0=Manual, 1=AutoSupported
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private int? _checkIntervalMinutes;
    
    // SiteType property that maps to index
    public SiteType SiteType
    {
        get => SiteTypeIndex == 1 ? SiteType.AutoSupported : SiteType.Manual;
        set => SiteTypeIndex = value == SiteType.AutoSupported ? 1 : 0;
    }
    
    // IsManualSite for visibility binding
    public bool IsManualSite => SiteTypeIndex == 0;
    
    partial void OnSiteTypeIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsManualSite));
        OnPropertyChanged(nameof(SiteType));
    }
    
    // Parser Config Properties
    [ObservableProperty] private int _selectorTypeIndex = 1; // 0=CSS, 1=XPath
    
    // SelectorType property that maps to index
    public SelectorType SelectorType
    {
        get => SelectorTypeIndex == 1 ? SelectorType.XPath : SelectorType.Css;
        set => SelectorTypeIndex = value == SelectorType.XPath ? 1 : 0;
    }
    
    partial void OnSelectorTypeIndexChanged(int value)
    {
        OnPropertyChanged(nameof(SelectorType));
    }
    [ObservableProperty] private string _listingItemSelector = "";
    [ObservableProperty] private string _titleSelector = "";
    [ObservableProperty] private string _priceSelector = "";
    [ObservableProperty] private string _urlSelector = "";
    [ObservableProperty] private string _dateSelector = "";
    [ObservableProperty] private string _listingIdSelector = "";
    [ObservableProperty] private string _encoding = "UTF-8";
    
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isEditing;
    
    // Window title
    public string WindowTitle => IsEditing ? "✏️ Site Düzenle" : "➕ Yeni Site Ekle";

    public SiteEditViewModel(SiteService siteService, Action onClose, Site? site = null)
    {
        _siteService = siteService;
        _onClose = onClose;
        _site = site;
        _isEditing = site != null;
        
        if (IsEditing)
        {
            LoadSiteData();
        }
    }

    private void LoadSiteData()
    {
        if (Site == null) return;
        
        Name = Site.Name;
        BaseUrl = Site.BaseUrl;
        SiteType = Site.SiteType;
        IsActive = Site.IsActive;
        CheckIntervalMinutes = Site.CheckIntervalMinutes;
        
        if (Site.ParserConfig != null)
        {
            SelectorType = Site.ParserConfig.SelectorType;
            ListingItemSelector = Site.ParserConfig.ListingItemSelector;
            TitleSelector = Site.ParserConfig.TitleSelector;
            PriceSelector = Site.ParserConfig.PriceSelector;
            UrlSelector = Site.ParserConfig.UrlSelector;
            DateSelector = Site.ParserConfig.DateSelector ?? "";
            ListingIdSelector = Site.ParserConfig.ListingIdSelector ?? "";
            Encoding = Site.ParserConfig.Encoding ?? "UTF-8";
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(BaseUrl))
            {
                Message = "Lütfen site adı ve URL giriniz.";
                return;
            }

            if (SiteType == SiteType.Manual)
            {
                if (string.IsNullOrWhiteSpace(ListingItemSelector) || 
                    string.IsNullOrWhiteSpace(TitleSelector) || 
                    string.IsNullOrWhiteSpace(UrlSelector))
                {
                    Message = "Manuel site için zorunlu seçicileri giriniz (İlan Kartı, Başlık, URL).";
                    return;
                }
            }

            if (IsEditing && Site != null)
            {
                // Update existing site
                Site.Name = Name;
                Site.BaseUrl = BaseUrl;
                Site.SiteType = SiteType;
                Site.IsActive = IsActive;
                Site.CheckIntervalMinutes = CheckIntervalMinutes;
                Site.UpdatedAt = DateTime.UtcNow;

                if (SiteType == SiteType.Manual)
                {
                    if (Site.ParserConfig == null)
                    {
                        Site.ParserConfig = new SiteParserConfig();
                    }
                    
                    Site.ParserConfig.SelectorType = SelectorType;
                    Site.ParserConfig.ListingItemSelector = ListingItemSelector?.Trim() ?? "";
                    Site.ParserConfig.TitleSelector = TitleSelector?.Trim() ?? "";
                    Site.ParserConfig.PriceSelector = PriceSelector?.Trim() ?? "";
                    Site.ParserConfig.UrlSelector = UrlSelector?.Trim() ?? "";
                    Site.ParserConfig.DateSelector = string.IsNullOrWhiteSpace(DateSelector) ? null : DateSelector.Trim();
                    Site.ParserConfig.ListingIdSelector = string.IsNullOrWhiteSpace(ListingIdSelector) ? null : ListingIdSelector.Trim();
                    Site.ParserConfig.Encoding = Encoding?.Trim() ?? "UTF-8";
                }

                await _siteService.UpdateSiteAsync(Site);
                Message = "Site başarıyla güncellendi!";
            }
            else
            {
                // Create new site
                var newSite = new Site
                {
                    Name = Name,
                    BaseUrl = BaseUrl,
                    SiteType = SiteType,
                    IsActive = IsActive,
                    CheckIntervalMinutes = CheckIntervalMinutes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (SiteType == SiteType.Manual)
                {
                    newSite.ParserConfig = new SiteParserConfig
                    {
                        SelectorType = SelectorType,
                        ListingItemSelector = ListingItemSelector?.Trim() ?? "",
                        TitleSelector = TitleSelector?.Trim() ?? "",
                        PriceSelector = PriceSelector?.Trim() ?? "",
                        UrlSelector = UrlSelector?.Trim() ?? "",
                        DateSelector = string.IsNullOrWhiteSpace(DateSelector) ? null : DateSelector.Trim(),
                        ListingIdSelector = string.IsNullOrWhiteSpace(ListingIdSelector) ? null : ListingIdSelector.Trim(),
                        Encoding = Encoding?.Trim() ?? "UTF-8"
                    };
                }

                await _siteService.AddSiteAsync(newSite);
                Message = "Site başarıyla eklendi!";
            }

            await Task.Delay(2000);
            _onClose?.Invoke();
        }
        catch (Exception ex)
        {
            Message = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onClose?.Invoke();
    }

    [RelayCommand]
    private void TestSelectors()
    {
        Message = "Seçici testi henüz implement edilmedi.";
    }
}
