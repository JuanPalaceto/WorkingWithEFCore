using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorkingWithEFCore.AutoGen;

[Index("CategoryName", Name = "CategoryName")] // No es necesario porque la base ya existía
public partial class Category
{
    [Key] // Indica que es una llave primaria, es INT por defecto
    [Column("CategoryID")] //Simplemente la está renombrando, eso creo
    public int CategoryId { get; set; }

    [StringLength(15)]
    public string CategoryName { get; set; } = null!;

    [Column(TypeName = "ntext")]
    public string? Description { get; set; }

    [Column(TypeName = "image")]
    public byte[]? Picture { get; set; }

    [InverseProperty("Category")] // Define la foreign key relationship
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
