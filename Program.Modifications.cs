using Microsoft.EntityFrameworkCore; // ExecuteUpdate, ExecuteDelete
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Packt.Shared; // Northwind, Product
using Microsoft.EntityFrameworkCore.Storage; // IDbContextTransaction

partial class Program
{
    // Lista los productos y los muestra por consola
    static void ListProducts(int[]? productIdsToHighlight = null) // parametro opcional que es un array de los ids a resaltar (color verde)
    {
        using (Northwind db = new())
        {
            if ((db.Products is null) || (!db.Products.Any()))
            {
                Fail("There are no products.");
                return;
            }

            // formatea la cadena, el segundo parametro son los caracteres de cada columna
            WriteLine("| {0,-3} | {1,-35} | {2,8} | {3,5} | {4} |",
            "Id", "Product Name", "Cost", "Stock", "Disc.");

            foreach (Product p in db.Products)
            {
                // Imprime los datos de la tabla. Si es el param a resaltar le cambia el color, si no solo la imprime formateado
                ConsoleColor previousColor = ForegroundColor;
                if ((productIdsToHighlight is not null) && productIdsToHighlight.Contains(p.ProductId))
                {
                    ForegroundColor = ConsoleColor.Green;
                }
                WriteLine("| {0:000} | {1,-35} | {2,8:$#,##0.00} | {3,5} | {4} |",
                p.ProductId, p.ProductName, p.Cost, p.Stock, p.Discontinued);
                ForegroundColor = previousColor;
            }
        }
    }

    // Agregar fila
    static (int affected, int productId) AddProduct(int categoryId, string productName, decimal? price) // acepta 3 parametros y regresa una tupla 
    {
        using (Northwind db = new())
        {
            if (db.Products is null) return (0, 0); // Si la tabla está vacíía regresa una tupla 0,0. o no se agregó nada

            // Se crea una instancia de Product, que es la fila a insertar en bd
            Product p = new()
            {
                CategoryId = categoryId,
                ProductName = productName,
                Cost = price,
                Stock = 72
            };

            // set product as added in change tracking
            // Se agrega el objeto p recien creado al contexto de base de datos con .Add
            EntityEntry<Product> entity = db.Products.Add(p);
            WriteLine($"State: {entity.State}, ProductId: {p.ProductId}"); //Esta del objeto (añadido) y su id

            // save tracked change to database
            // el metodo savechanges guarda los cambios en la bd. Aquí es que se agregar en la bd y actualiza su id si es necesario. Devuelve las filas afectadas
            int affected = db.SaveChanges();
            WriteLine($"State: {entity.State}, ProductId: {p.ProductId}"); // Volvemos a mostrar el estado del objeto entity (ahora ya tiene un id dado por la bd)
            return (affected, p.ProductId);
        }
    }

    // ACTUALIZAR REGISTRO (incrementa precio(
    // Igual y la tupla se podría poner aparte si se comienza a repetir mucho(?
    static (int affected, int productId) IncreaseProductPrice(string productNameStartsWith, decimal amount)
    {
        using (Northwind db = new())
        {
            if (db.Products is null) return (0, 0);

            // Get the first product whose name starts with the parameter value.
            Product updateProduct = db.Products.First(p => p.ProductName.StartsWith(productNameStartsWith));
            updateProduct.Cost += amount;
            int affected = db.SaveChanges();
            return (affected, updateProduct.ProductId);
        }
    }

    // Borrar registros que empiecen por la cadena sumisntrada
    static int DeleteProducts(string productNameStartsWith)
    {
        using (Northwind db = new())
        {
            using (IDbContextTransaction t = db.Database.BeginTransaction())
            {
                WriteLine("Transaction isolation level: {0}", arg0: t.GetDbTransaction().IsolationLevel); // Trabajando con trasacciones e isolacion(?

                IQueryable<Product>? products = db.Products?.Where(p => p.ProductName.StartsWith(productNameStartsWith));

                if ((products is null) || (!products.Any()))
                {
                    WriteLine("No products found to delete.");
                    return 0;
                }
                else
                {
                    if (db.Products is null) return 0;
                    {
                        db.Products.RemoveRange(products); // O remove solamente si fuera uno solo
                    }
                }
                int affected = db.SaveChanges();
                t.Commit();
                return affected;
            }
        }
    }

    // OTRAS FORMAS DE BORRAR Y ACTUALIZAR
    // Estos solo pueden borrar o actualizar de una sola tabla, a pesar de poder escribir queries más complejas

    // Esto ejecutaría DELETE FROM Products
    // await db.Products.ExecuteDeleteAsync();

    // Esto ejecutaría DELETE FROM Products p WHERE p.UnitPrice > 50
    // await db.Products.Where(product => product.UnitPrice > 50).ExecuteDeleteAsync();

    // Esto haría un update en todos los productos que no estén descontinuados e incrementaría su precio un 10% debido a la inflación, por ejemplo
    //await db.Products
    //    .Where(product => !product.Discontinued)
    //    .ExecuteUpdateAsync(s => s.SetProperty(
    //    p => p.UnitPrice, // Selects the property to update.
    //    p => p.UnitPrice* 0.1)); // Sets the value to update it to.

    // Actualiza todos los producos que su nombre empiece con un string dado usando ExecuteUpdate
    static (int affected, int[]? productIds) IncreaseProductPricesBetter(string productNameStartsWith, decimal amount)
    {
        using (Northwind db = new())
        {
            if (db.Products is null) return (0, null);

            // Get products whose name starts with the parameter value.
            IQueryable<Product>? products = db.Products.Where(
            p => p.ProductName.StartsWith(productNameStartsWith));

            int affected = products.ExecuteUpdate(s => s.SetProperty(
            p => p.Cost, // Property selector lambda expression.
            p => p.Cost + amount)); // Value to update to lambda expression.

            int[] productIds = products.Select(p => p.ProductId).ToArray();
            return (affected, productIds);
        }
    }

    // Eliminar todos los productos con un nombre dado usando ExecuteDelete
    static int DeleteProductsBetter(string productNameStartsWith)
    {
        using (Northwind db = new())
        {
            int affected = 0;

            IQueryable<Product>? products = db.Products?.Where(
            p => p.ProductName.StartsWith(productNameStartsWith));

            if ((products is null) || (!products.Any()))
            {
                WriteLine("No products found to delete.");
                return 0;
            }
            else
            {
                affected = products.ExecuteDelete();
            }
            return affected;
        }
    }
}