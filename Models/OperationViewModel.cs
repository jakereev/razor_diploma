using System;
using System.Collections.Generic;

namespace AlphaMaterials.Models
{
    public class OperationViewModel
    {
        public int Id { get; set; }                 // для Edit
        public string Type { get; set; } = null!;
        public string? DocumentNo { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }

        // Список строк — теперь List, и индексирование работает!
        public List<OperationRowViewModel> Rows { get; set; } = new();
    }
}
