namespace AlphaMaterials.Models
{
    public class OperationRow
    {
        public int Id { get; set; }

        public int OperationId { get; set; }
        public Operation Operation { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
