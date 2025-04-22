using RAGEcommerce.Models;

namespace RAGEcommerce.Services;

public class ProductCatalogService
{
    private readonly List<Product> products =
    [
        new Product
            { Id = 1, Name = "Running Shoes", Description = "Lightweight shoes for running." },

        new Product
            { Id = 2, Name = "Hiking Boots", Description = "Durable boots for mountain trails." },

        new Product { Id = 3, Name = "Yoga Mat", Description = "Non-slip mat for yoga practice." },
        new Product
        {
            Id = 4, Name = "Fitness Tracker",
            Description = "Wearable device to monitor health and activity."
        },

        new Product
        {
            Id = 5, Name = "Water Bottle",
            Description = "Insulated bottle for keeping drinks cool during workouts."
        },

        new Product
        {
            Id = 6, Name = "Resistance Bands",
            Description = "Set of bands for strength training exercises."
        },

        new Product
        {
            Id = 7, Name = "Cycling Helmet",
            Description = "Protective helmet designed for cycling safety."
        }
    ];

    public List<Product> GetAllProducts() => this.products;
}
