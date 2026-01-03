-- İlanburda ve Bursverenler için daha iyi XPath'ler

-- İlanburda.net - tüm ilan linklerini çek
UPDATE SiteParserConfigs
SET 
    ListingItemSelector = '//a[contains(@href, "https://www.ilanburda.net/") and not(contains(@href, "/sayfa/")) and not(contains(@href, "/magaza/"))]',
    TitleSelector = './h3/text() | ./text()',
    UrlSelector = './@href'
WHERE SiteId = (SELECT Id FROM Sites WHERE Name = 'İlanburda.net');

-- Bursverenler.org - link pattern'e göre
UPDATE SiteParserConfigs
SET 
    ListingItemSelector = '//a[contains(@href, "bursverenler.org/") and not(contains(@href, "web/site")) and string-length(@href) > 30]',
    TitleSelector = './text()',
    UrlSelector = './@href'
WHERE SiteId = (SELECT Id FROM Sites WHERE Name = 'Bursverenler.org');

SELECT 'Updated' as Status;
