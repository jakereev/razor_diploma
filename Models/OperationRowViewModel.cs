namespace AlphaMaterials.Models
{
    public class OperationRowViewModel
    {
        // Здесь можно сохранять Id существующей строки, если нужно редактировать
        public int? Id { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
