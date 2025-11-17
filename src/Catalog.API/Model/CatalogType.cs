using System.ComponentModel.DataAnnotations;

namespace IronExchange.Catalog.API.Model;

public class CatalogType
{
    public CatalogType(string type) {
        Type = type;
    }

    public int Id { get; set; }

    [Required]
    public string Type { get; set; }
}
