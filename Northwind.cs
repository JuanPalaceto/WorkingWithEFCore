using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Packt.Shared;
public class Northwind : DbContext
{
    // Para usar esto hay que agregar el paquete EntityFrameworkCore.Design
    // Esto es el string usado dotnet-ef-tool (esto va en la terminal(
    // scaffolding (?
    // dotnet ef dbcontext scaffold "Server=localhost;Database=Northwind;User Id=sa;Password=12345;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --table Categories --table Products --output-dir AutoGenModels --namespace WorkingWithEFCore.AutoGen --data-annotations --context Northwind


    // Estas propiedades mapean las tablas de la bd
    // Estas tablas son las que cree en las clases
    public DbSet<Category>? Categories { get; set; }
    public DbSet<Product>? Products { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server=localhost;Database=Northwind;User Id=sa;Password=12345;TrustServerCertificate=True";

        ConsoleColor previousColor = ForegroundColor;
        ForegroundColor = ConsoleColor.DarkYellow;

        WriteLine($"Connection: {connectionString}");
        ForegroundColor = previousColor;

        optionsBuilder.UseSqlServer(connectionString);
        // logs (también me da el query, supongo que poir el CommandExecuting)
        optionsBuilder.LogTo(WriteLine, new[] {RelationalEventId.CommandExecuting}).EnableSensitiveDataLogging(); // console
        // Se supone que al instalar el paquete Microsoft.EntityFrameworkCore.Proxies esto activa la carga peresoza o lazy, que en teoría carga los datos, pero tiene un problema porque muestra de uno en uno las categorias
        // optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // example of using Fluent API instead of attributes
        // to limit the length of a category name to 15
        // el equivalente a esto
        // [Required]
        // [StringLength(15)]
        modelBuilder.Entity<Category>()
        .Property(category => category.CategoryName)
        .IsRequired() // NOT NULL
        .HasMaxLength(15);
        // Esto no me sirve porque es para sqlite, para la conversion de tipos porque en sqlite se manejan distinto los numericos
        //if (Database.ProviderName?.Contains("Sqlite") ?? false)
        //{
        //    // added to "fix" the lack of decimal support in SQLite
        //    modelBuilder.Entity<Product>()
        //    .Property(product => product.Cost)
        //    .HasConversion<double>();
        //}

        // filtro global para quitar los productos descontinuados de los resultados
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.Discontinued);
    }

    // Convención de modelos para por ejemplo, que todas las propiedades string tengan max 50 caracteres ó evitar que se mapean propiedades que implemente interfaces personalizadas
    //protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    //{
    //    configurationBuilder.Properties<string>().HaveMaxLength(50);
    //    configurationBuilder.IgnoreAny<IDoNotMap>();
    //}
}
