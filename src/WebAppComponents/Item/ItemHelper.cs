using IronExchange.WebAppComponents.Catalog;


namespace IronExchange.WebAppComponents.Item;

public static class ItemHelper
{
    public static string Url(CatalogItem item)
        => $"item/{item.Id}";
}
