using Microsoft.EntityFrameworkCore; // Include extension method
using Microsoft.IdentityModel.Tokens;
using Packt.Shared; // Northwind, Category y Product classes pertenecen a este namespace
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking; //CollectionEntry - explicit loading

partial class Program
{
    // Usando Include para los joins(?
    static void QueryingCategories()
    {
        using (Northwind db = new Northwind())
        {
            SectionTitle("Categories and how many products they have:");

            // query que obtiene las categorías y sus productos relacionados
            // IQueryable es una interfaz que representa una consulta que se puede ejecutar contra una fuente de datos
            // La función Include se utiliza para especificar que también se deben cargar los productos relacionados con cada categoría (carga ansiosa o eager)
            IQueryable<Category> categories = db.Categories?.Include(c => c.Products);
            //IQueryable<Category>? categories = db.Categories; // ejemplo sin carga ansiosa o eager (sin include(
            //IQueryable<Category>? categories; // ejemplo de carga explicita
            //db.ChangeTracker.LazyLoadingEnabled = false;

            //Write("Enable eager loading? (Y/N): ");
            //bool eagerLoading = (ReadKey(intercept: true).Key == ConsoleKey.Y);
            //bool explicitLoading = false;
            //WriteLine();

            //if (eagerLoading)
            //{
            //    categories = db.Categories?.Include(c => c.Products);
            //}
            //else
            //{
            //    categories = db.Categories;
            //    Write("Enable explicit loading? (Y/N): ");
            //    explicitLoading = (ReadKey(intercept: true).Key == ConsoleKey.Y);
            //    WriteLine();
            //}

            // Carga ansiosa
            if ((categories is null) || (!categories.Any()))
            {
                Fail("No categories found.");
                return;
            }

            // Ejecutar el query y enumerar los resultados
            foreach (Category c in categories)
            {
                // Carga explicita
                //if (explicitLoading)
                //{
                //    Write($"Explicitly load products for {c.CategoryName}? (Y/N): ");
                //    ConsoleKeyInfo key = ReadKey(intercept: true);
                //    WriteLine();
                //    if (key.Key == ConsoleKey.Y)
                //    {
                //        CollectionEntry<Category, Product> products =
                //        db.Entry(c).Collection(c2 => c2.Products);
                //        if (!products.IsLoaded) products.Load();
                //    }
                //}
                // Carga ansiosa
                WriteLine($"{c.CategoryName} has {c.Products.Count} products.");
            }
        }
    }

    // Usando WHERE
    static void FilteredIncludes()
    {
        using (Northwind db = new())
        {
            SectionTitle("Products with a minimun number of units in stock.");

            string? input;
            int stock;

            // Ciclo para que el usuario introduzca un numero valido
            do
            {
                Write("Enter a minimun for units in stock: ");
                input = ReadLine();
            } while (!int.TryParse(input, out stock)); // convierte la entrada string en un int y lo almacena en la variable stock

            // Crear la consulta, busca categorias en la bd y los productos que tiene relacion con estos. También utiliza un filtro usando Where, donde solo traerá los productos con un stock igual o mayor al introducido
            IQueryable<Category>? categories = db.Categories?.Include(c => c.Products.Where(p => p.Stock >= stock));

            if ((categories is null) || (!categories.Any()))
            {
                Fail("No categories found.");
                return;
            }

            // Generar el SQL
            Info($"ToQueryString: {categories.ToQueryString()}");

            // Esto simplemente recorre todos los elementos y lso imprime con diferentes mensajes
            foreach (Category c in categories)
            {
                WriteLine($"{c.CategoryName} has {c.Products.Count} products with a minimum of {stock} units in stock.");

                foreach (Product p in c.Products)
                {
                    WriteLine($" {p.ProductName} has {p.Stock} units in stock.");
                }
            }
        }
    }

    // Usando OrderBy
    static void QueryingProducts()
    {
        using (Northwind db = new())
        {
            SectionTitle("Products that cost more than a price, highest at top.");

            string? input;
            decimal price;

            do
            {
                Write("Enter a product price: ");
                input = ReadLine();
            } while (!decimal.TryParse(input, out price));

            // Se puede hacer de forma de multilínea, de esta forma es más legible
            IQueryable<Product>? products = db.Products?
            .TagWith("Products Filtered by price and sorted.") // agregar un tag o etiqueta al query(?
            .Where(product => product.Cost > price)
            .OrderByDescending(product => product.Cost); // Esta es la única línea nueva, todo esto es de LinQ. Es para ordenar además de filtrar. Ordena por costo de forma descendiente

            // Esto se comienza a repetir mucho en los ejemplos, podría llegar a hacerse un metodo(?
            // Es más eficiente checar con !products.Any() que usar products.Count() == 0
            if ((products is null) || (!products.Any()))
            {
                Fail("No products found.");
                return;
            }

            // Generar el SQL
            Info($"ToQueryString: {products.ToQueryString()}");

            foreach (Product p in products)
            {
                // Formatear cadenas de texto.
                //WriteLine($"{p.ProductId}: {p.ProductName} costs {p.Cost:C} and has {p.Stock} in stock."); En teoría esto sería lo mismo. :C lo convierte a moneda del sistema.
                WriteLine("{0}: {1} costs {2:$#,##0.00} and has {3} in stock.", p.ProductId, p.ProductName, p.Cost, p.Stock);
            }
        }
    }

    // Usando LIKE
    static void QueryingWithLike()
    {
        using (Northwind db = new())
        {
            SectionTitle("Pattern matching with LIKE.");
            Write("Enter part of a product name: ");
            string? input = ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Fail("You did not enter part of a product name.");
                return;
            }

            IQueryable<Product>? products = db.Products?
            .Where(p => EF.Functions.Like(p.ProductName, $"%{input}%")); // EF.Functions.Like() es una función de EF para utilizar LIKE en SQL Server. Aqui busca en mi lista de productos resultado del query, los que contengan alguna parte de la cadena de texto de la entrada.

            if ((products is null) || (!products.Any()))
            {
                Fail("No products found.");
                return;
            }

            foreach (Product p in products)
            {
                WriteLine("{0} has {1} units in stock. Discontinued? {2}", p.ProductName, p.Stock, p.Discontinued);
            }
        }
    }

    // Generar un numero aleatorio en queries
    static void GetRandomProduct()
    {
        using (Northwind db = new())
        {
            SectionTitle("Get a random product.");
            int? rowCount = db.Products?.Count();

            if (rowCount == null)
            {
                Fail("Products table is empty.");
                return;
            }

            // First or defaulta para que solo traiga uno
            // Y la función EF.Functions.Random sirve parao btener un numero aleatorio
            Product? p = db.Products?.FirstOrDefault(p => p.ProductId == (int)(EF.Functions.Random() * rowCount));
            if (p == null)
            {
                Fail("Product not found.");
                return;
            }
            WriteLine($"Random product: {p.ProductId} {p.ProductName}");
        }
    }
}