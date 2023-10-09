using System.ComponentModel.DataAnnotations; // para usar [Required], [StringLength]
using System.ComponentModel.DataAnnotations.Schema; // para usar [Column]

namespace Packt.Shared;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(40)]
    public string ProductName { get; set; } = null!; // Se inicia como nula para ignorar la advertencia

    [Column("UnitPrice", TypeName = "money")]
    public decimal? Cost { get; set; } // Se puede cambiar el nombre de la propiedad (Cost) al de la columna en sql server (UnitPrice)

    [Column ("UnitsInStock")]
    public short? Stock { get; set; }

    public bool Discontinued { get; set; }

    // Estas definen la relacion de las claves foraneas con la tbla de categoria
    // Siguen siendo propiedades
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!; // Tanto esta como la de categoria son virtuales para añadir funciones extra

}
