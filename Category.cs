using System.ComponentModel.DataAnnotations.Schema; // para usar [Column]

namespace Packt.Shared;

public class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    [Column(TypeName = "ntext")]
    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; }

    public Category() {
        Products = new HashSet<Product>(); 
    }
}
