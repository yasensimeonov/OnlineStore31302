using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using Core.Entities;
using System;
using Infrastructure.Helpers;
using Core.Entities.OrderAggregate;
using System.Reflection;

namespace Infrastructure.Data
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                if (!context.ProductBrands.Any())
                {
                    var brandsData = File.ReadAllText("../Infrastructure/Data/SeedData/brands.json");

                    var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

                    foreach (var item in brands)
                    {
                        context.ProductBrands.Add(item);                        
                    }

                    // await context.SaveChangesAsync();
                    await context.SaveChangesWithIdentityInsert<ProductBrand>();
                }

                if (!context.ProductTypes.Any())
                {
                    var typesData = File.ReadAllText("../Infrastructure/Data/SeedData/types.json");

                    var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);

                    foreach (var item in types)
                    {
                        context.ProductTypes.Add(item);                        
                    }

                    // await context.SaveChangesAsync();
                    await context.SaveChangesWithIdentityInsert<ProductType>();
                }

                if (!context.DeliveryMethods.Any())
                {
                    var dmData = File.ReadAllText("../Infrastructure/Data/SeedData/delivery.json");

                    var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(dmData);

                    foreach (var item in deliveryMethods)
                    {
                        context.DeliveryMethods.Add(item);                        
                    }

                    // await context.SaveChangesAsync();
                    await context.SaveChangesWithIdentityInsert<DeliveryMethod>();
                }                

                if (!context.Products.Any())
                {
                    // var productsData = File.ReadAllText("../Infrastructure/Data/SeedData/products.json");
                    var productsData = File.ReadAllText(path + @"/Data/SeedData/products.json");

                    // var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                    var products = JsonSerializer.Deserialize<List<ProductSeedModel>>(productsData);

                    foreach (var item in products)
                    {
                        var pictureFileName = item.PictureUrl.Substring(16);
                        var product = new Product
                        {
                            Name = item.Name,
                            Description = item.Description,
                            Price = item.Price,
                            ProductBrandId = item.ProductBrandId,
                            ProductTypeId = item.ProductTypeId
                        };
                        product.AddPhoto(item.PictureUrl, pictureFileName);                        
                        
                        // context.Products.Add(item);
                        context.Products.Add(product);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<StoreContextSeed>();
                logger.LogError(ex.Message);
            }
        }
    }
}