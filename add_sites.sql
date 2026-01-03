-- Manuel Site Konfigürasyonları
-- Bu scriptler 3 sitenin ManualSiteScraper için tanımlarını ekler

-- 1. Microfon.co - Burslar
INSERT INTO Sites (Name, BaseUrl, SiteType, IsActive, CreatedAt, UpdatedAt)
VALUES ('Microfon Burslar', 'https://microfon.co/scholarship', 2, 1, datetime('now'), datetime('now'));

INSERT INTO SiteParserConfigs (
    SiteId, 
    ListingItemSelector, 
    TitleSelector, 
    PriceSelector, 
    UrlSelector, 
    DateSelector,
    ListingIdSelector,
    SelectorType, 
    Encoding
)
VALUES (
    (SELECT Id FROM Sites WHERE Name = 'Microfon Burslar'),
    '//div[contains(@class, "slick-slide")]//a[@class="styled-desk-title"]/ancestor::div[2]',
    './/a[@class="styled-desk-title"]',
    '',  -- Fiyat yok
    './/a[@class="styled-desk-title"]/@href',
    './/div[contains(@class, "sc-bd9b1678-18")]/div[last()-1]',
    '',  -- ID URL'den çıkarılacak
    2,  -- XPath
    'utf-8'
);

-- 2. Bursverenler.org - Aktif Burslar
INSERT INTO Sites (Name, BaseUrl, SiteType, IsActive, CreatedAt, UpdatedAt)
VALUES ('Bursverenler.org', 'https://bursverenler.org/?lang=tr', 2, 1, datetime('now'), datetime('now'));

INSERT INTO SiteParserConfigs (
    SiteId, 
    ListingItemSelector, 
    TitleSelector, 
    PriceSelector, 
    UrlSelector, 
    DateSelector,
    ListingIdSelector,
    SelectorType, 
    Encoding
)
VALUES (
    (SELECT Id FROM Sites WHERE Name = 'Bursverenler.org'),
    '//h3[text()="Aktif Burslar"]/following-sibling::a',
    '.',
    '',
    '@href',
    '',
    '',
    2,  -- XPath
    'utf-8'
);

-- 3. İlanburda.net - Son İlanlar
INSERT INTO Sites (Name, BaseUrl, SiteType, IsActive, CreatedAt, UpdatedAt)
VALUES ('İlanburda.net', 'https://www.ilanburda.net/', 2, 1, datetime('now'), datetime('now'));

INSERT INTO SiteParserConfigs (
    SiteId, 
    ListingItemSelector, 
    TitleSelector, 
    PriceSelector, 
    UrlSelector, 
    DateSelector,
    ListingIdSelector,
    SelectorType, 
    Encoding
)
VALUES (
    (SELECT Id FROM Sites WHERE Name = 'İlanburda.net'),
    '//h3[text()="Son İlanlar"]/following-sibling::a[contains(@href, "https://www.ilanburda.net/")]',
    '.',
    'preceding-sibling::a[1]',  -- Bir önceki link fiyat
    '@href',
    '',
    '',
    2,  -- XPath
    'utf-8'
);

-- Kontrol
SELECT 
    s.Id,
    s.Name,
    s.BaseUrl,
    spc.ListingItemSelector
FROM Sites s
LEFT JOIN SiteParserConfigs spc ON s.Id = spc.SiteId
WHERE s.SiteType = 2;
