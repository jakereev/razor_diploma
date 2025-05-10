using System;
using System.Linq;
using System.Threading.Tasks;
using AlphaMaterials.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AlphaMaterials.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _db;

        static ReportService()
        {
            // Устанавливаем лицензию EPPlus 8+
            ExcelPackage.License.SetNonCommercialOrganization("AlphaMaterials");
        }

        public ReportService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GenerateStockReportAsync()
        {
            var items = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Select(p => new {
                    p.Name,
                    Category = p.Category.Name,
                    Supplier = p.Supplier.Name,
                    p.StockQuantity
                })
                .ToListAsync();

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Остатки");

            // Шапка
            sheet.Cells[1, 1].Value = "Название";
            sheet.Cells[1, 2].Value = "Категория";
            sheet.Cells[1, 3].Value = "Поставщик";
            sheet.Cells[1, 4].Value = "Остаток";

            // Данные
            for (int i = 0; i < items.Count; i++)
            {
                int row = i + 2;
                sheet.Cells[row, 1].Value = items[i].Name;
                sheet.Cells[row, 2].Value = items[i].Category;
                sheet.Cells[row, 3].Value = items[i].Supplier;
                sheet.Cells[row, 4].Value = items[i].StockQuantity;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        /// <inheritdoc/>
        public async Task<byte[]> GenerateOperationsReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? type,
            int? employeeId)
        {
            var query = _db.Operations
                .Include(o => o.Employee)
                .Include(o => o.Rows).ThenInclude(r => r.Product)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(o => o.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(o => o.Date <= toDate.Value);
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(o => o.Type == type);
            if (employeeId.HasValue)
                query = query.Where(o => o.EmployeeId == employeeId.Value);

            var list = await query
                .OrderByDescending(o => o.Date)
                .Select(o => new {
                    o.Date,
                    o.Type,
                    o.DocumentNo,
                    Employee = o.Employee.FullName,
                    ItemsCount = o.Rows.Count
                })
                .ToListAsync();

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Операции");

            // Шапка
            sheet.Cells[1, 1].Value = "Дата";
            sheet.Cells[1, 2].Value = "Тип";
            sheet.Cells[1, 3].Value = "Документ";
            sheet.Cells[1, 4].Value = "Сотрудник";
            sheet.Cells[1, 5].Value = "Кол-во позиций";

            // Данные
            for (int i = 0; i < list.Count; i++)
            {
                int row = i + 2;
                sheet.Cells[row, 1].Style.Numberformat.Format = "гггг-мм-дд";
                sheet.Cells[row, 1].Value = list[i].Date;
                sheet.Cells[row, 2].Value = list[i].Type;
                sheet.Cells[row, 3].Value = list[i].DocumentNo;
                sheet.Cells[row, 4].Value = list[i].Employee;
                sheet.Cells[row, 5].Value = list[i].ItemsCount;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
