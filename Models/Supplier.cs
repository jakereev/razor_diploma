namespace AlphaMaterials.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Inn { get; set; }          // ИНН (необязательно)
        public string? Address { get; set; }      // Адрес (необязательно)
    }
}
