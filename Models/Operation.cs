namespace AlphaMaterials.Models
{
    public class Operation
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!; // например: "Purchase","Sale","Transfer","WriteOff"
        public string? DocumentNo { get; set; }
        public DateTime Date { get; set; }

        // Привязка к сотруднику
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Строки операции
        public ICollection<OperationRow> Rows { get; set; } = new List<OperationRow>();
    }
}
