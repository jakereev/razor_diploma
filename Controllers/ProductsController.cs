using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlphaMaterials.Data;
using AlphaMaterials.Models;

namespace AlphaMaterials.Controllers
{
    [Authorize]  // для просмотра списка и деталей нужен хотя бы логин
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        [AllowAnonymous]  // каталог доступен гостям
        public async Task<IActionResult> Index(string? search, int? categoryId, int? supplierId,
                                               string? sortOrder, int pageNumber = 1)
        {
            var query = _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.Supplier)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);
            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId);

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SupplierId = supplierId;

            ViewBag.NameSortParam = sortOrder == "Name" ? "Name_desc" : "Name";
            ViewBag.CategorySortParam = sortOrder == "Category" ? "Category_desc" : "Category";
            ViewBag.SupplierSortParam = sortOrder == "Supplier" ? "Supplier_desc" : "Supplier";
            ViewBag.PriceSortParam = sortOrder == "Price" ? "Price_desc" : "Price";
            ViewBag.CurrentSort = sortOrder;

            query = sortOrder switch
            {
                "Name_desc" => query.OrderByDescending(p => p.Name),
                "Name" => query.OrderBy(p => p.Name),
                "Category_desc" => query.OrderByDescending(p => p.Category.Name),
                "Category" => query.OrderBy(p => p.Category.Name),
                "Supplier_desc" => query.OrderByDescending(p => p.Supplier.Name),
                "Supplier" => query.OrderBy(p => p.Supplier.Name),
                "Price_desc" => query.OrderByDescending(p => p.Price),
                "Price" => query.OrderBy(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name", categoryId);
            ViewBag.Suppliers = new SelectList(
                await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(),
                "Id", "Name", supplierId);

            return View(items);
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View();
        }

        // POST: Products/Create
        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CategoryId,SupplierId,Price,StockQuantity")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDownsAsync();
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            await PopulateDropDownsAsync();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId,SupplierId,Price,StockQuantity")] Product product)
        {
            if (id != product.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDownsAsync();
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDownsAsync()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name");
            ViewBag.Suppliers = new SelectList(
                await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(),
                "Id", "Name");
        }
    }
}
