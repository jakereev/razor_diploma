namespace AlphaMaterials.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        // Связь с Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Связь с Supplier
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
    }
}
