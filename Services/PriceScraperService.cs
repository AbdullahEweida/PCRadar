using HtmlAgilityPack;

namespace PCRadar.Services
{
    public class PriceScraperService : IPriceScraperService
    {
        public async Task<List<ScrapedProduct>> ScrapeCategoryAsync(string categoryUrl, string cardXPath, string nameXPath, string priceXPath, string linkXPath, string availabilityXPath)
        {
            var allScrapedProducts = new List<ScrapedProduct>();
            var web = new HtmlWeb();

            int pageNumber = 1;
            bool hasMoreProducts = true;
            string lastPageFirstItemName = "";

            while (hasMoreProducts && pageNumber <= 20)
            {
                try
                {
                    string separator = categoryUrl.Contains("?") ? "&" : "?";
                    string pagedUrl = $"{categoryUrl}{separator}page={pageNumber}";

                    Console.WriteLine($"[Scraper] Scraping page {pageNumber}: {pagedUrl}");

                    var document = await web.LoadFromWebAsync(pagedUrl);
                    var productCards = document.DocumentNode.SelectNodes(cardXPath);

                    if (productCards == null || productCards.Count == 0)
                    {
                        hasMoreProducts = false;
                        break;
                    }

                    // استخراج اسم أول كارت في الصفحة الحالية
                    var firstCardNameNode = productCards.FirstOrDefault()?.SelectSingleNode(nameXPath);
                    string currentFirstItemName = firstCardNameNode?.InnerText.Trim() ?? "";

                    // لو أول منتج هو هو أول منتج في الصفحة اللي فاتت، يبقى إحنا دخلنا في Infinite Loop
                    if (!string.IsNullOrEmpty(currentFirstItemName) && currentFirstItemName == lastPageFirstItemName)
                    {
                        Console.WriteLine($"[Scraper] Infinite loop detected at page {pageNumber}. Stopping scraping for this category.");
                        break;
                    }

                    // تحديث المتغير للصفحة الجاية
                    lastPageFirstItemName = currentFirstItemName;

                    foreach (var card in productCards)
                    {
                        try
                        {
                            var nameNode = card.SelectSingleNode(nameXPath);
                            var priceNode = card.SelectSingleNode(priceXPath);
                            var linkNode = card.SelectSingleNode(linkXPath);
                            var availabilityNode = card.SelectSingleNode(availabilityXPath);

                            if (nameNode != null && priceNode != null && linkNode != null)
                            {
                                bool isAvailable = true;
                                if (availabilityNode != null)
                                {
                                    // 1. حالة موقع AHW (بيستخدم زرار button للمنتج المتاح أو المخلصان)
                                    if (availabilityNode.Name == "button")
                                    {
                                        // نعتمد على خاصية disabled فقط ونتجاهل النص تماماً عشان نتفادى النص المخفي
                                        var disabledAttr = availabilityNode.GetAttributeValue("disabled", null);
                                        isAvailable = (disabledAttr == null);
                                    }
                                    else
                                    {
                                        string nodeText = availabilityNode.InnerText.ToLower();
                                        if (nodeText.Contains("coming soon") || nodeText.Contains("out of stock") || nodeText.Contains("pc build component"))
                                        {
                                            isAvailable = false;
                                        }
                                        else
                                        {
                                            isAvailable = true;
                                        }
                                    }
                                }

                                    string rawPrice = priceNode.InnerText.Replace(",", "").Trim();
                                var match = System.Text.RegularExpressions.Regex.Match(rawPrice, @"\d+(\.\d+)?");

                                if (match.Success && decimal.TryParse(match.Value, out decimal price))
                                {
                                    if (price <= 1)
                                    {
                                        isAvailable = false;
                                    }

                                    string productUrl = linkNode.GetAttributeValue("href", "");

                                    if (!productUrl.StartsWith("http"))
                                    {
                                        var uri = new Uri(categoryUrl);
                                        productUrl = $"{uri.Scheme}://{uri.Host}{productUrl}";
                                    }

                                    allScrapedProducts.Add(new ScrapedProduct
                                    (
                                        nameNode.InnerText.Trim(),
                                        productUrl,
                                        price,
                                        isAvailable
                                    ));
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    pageNumber++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on page {pageNumber}: {ex.Message}");
                    hasMoreProducts = false;
                }
            }

            return allScrapedProducts;
        }
    }
}