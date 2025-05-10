using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlphaMaterials.Data;
using AlphaMaterials.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;


namespace AlphaMaterials.Controllers
{
    [Authorize]
    public class OperationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public OperationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Operations
        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate,
            string type,
            int? employeeId,
            int page = 1)
        {
            var query = _context.Operations
                .Include(o => o.Employee)
                .Include(o => o.Rows)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.Date <= toDate.Value);
            if (!string.IsNullOrEmpty(type)) query = query.Where(o => o.Type == type);
            if (employeeId.HasValue) query = query.Where(o => o.EmployeeId == employeeId.Value);

            int totalCount = await query.CountAsync();

            var list = await query
                .OrderByDescending(o => o.Date)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Type = type;
            ViewBag.EmployeeId = employeeId;

            ViewBag.Types = new SelectList(new[]
            {
                new { Value = "",         Text = "Все типы"   },
                new { Value = "Purchase", Text = "Закупка"     },
                new { Value = "Sale",     Text = "Продажа"     },
                new { Value = "WriteOff", Text = "Списание"    },
            }, "Value", "Text", type);

            ViewBag.Employees = new SelectList(
                await _context.Employees.OrderBy(e => e.FullName).ToListAsync(),
                "Id", "FullName", employeeId);

            return View(list);
        }

        // GET: /Operations/Export
        public async Task<FileResult> Export(
            DateTime? fromDate,
            DateTime? toDate,
            string type,
            int? employeeId)
        {
            var query = _context.Operations
                .Include(o => o.Employee)
                .Include(o => o.Rows)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.Date <= toDate.Value);
            if (!string.IsNullOrEmpty(type)) query = query.Where(o => o.Type == type);
            if (employeeId.HasValue) query = query.Where(o => o.EmployeeId == employeeId.Value);

            var list = await query.OrderByDescending(o => o.Date).ToListAsync();

            // ==== Здесь правим лицензию EPPlus 8+ ====
            // Используем либо личную, либо организационную непрофессиональную лицензию:
            ExcelPackage.License.SetNonCommercialPersonal("ВашаОрганизацияИлиИмя");
            // или ExcelPackage.License.SetNonCommercialOrganization("ООО Альфа");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Operations");

            ws.Cells[1, 1].Value = "Дата";
            ws.Cells[1, 2].Value = "Тип";
            ws.Cells[1, 3].Value = "Документ";
            ws.Cells[1, 4].Value = "Сотрудник";
            ws.Cells[1, 5].Value = "Кол-во позиций";

            for (int i = 0; i < list.Count; i++)
            {
                var op = list[i];
                int r = i + 2;
                ws.Cells[r, 1].Value = op.Date.ToString("dd.MM.yyyy");
                ws.Cells[r, 2].Value = op.Type switch
                {
                    "Purchase" => "Закупка",
                    "Sale" => "Продажа",
                    "WriteOff" => "Списание",
                    _ => op.Type
                };
                ws.Cells[r, 3].Value = op.DocumentNo;
                ws.Cells[r, 4].Value = op.Employee.FullName;
                ws.Cells[r, 5].Value = op.Rows.Count;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Operations_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
