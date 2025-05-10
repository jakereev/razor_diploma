using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AlphaMaterials.Services;

namespace AlphaMaterials.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reports;
        public ReportsController(IReportService reports)
        {
            _reports = reports;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/Stock
        public async Task<IActionResult> Stock()
        {
            var bytes = await _reports.GenerateStockReportAsync();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "StockReport.xlsx");
        }
    }
}
