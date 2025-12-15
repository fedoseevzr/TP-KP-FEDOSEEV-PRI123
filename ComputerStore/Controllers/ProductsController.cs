using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ComputerStore.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        private void PopulateDropdowns(object selectedCategory = null, object selectedSupplier = null)
        {
            var categoriesQuery = _context.Categories.OrderBy(c => c.Name);
            var suppliersQuery = _context.Suppliers.OrderBy(s => s.Name);

            ViewData["CategoryId"] = new SelectList(categoriesQuery, "Id", "Name", selectedCategory);
            ViewData["SupplierId"] = new SelectList(suppliersQuery, "Id", "Name", selectedSupplier);
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier);

            return View(await appDbContext.ToListAsync());
        }

        // GET: /Products/Details/5
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

        // GET: /Products/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: /Products/Create
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Price,CategoryId,SupplierId")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.Quantity = 0;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        // GET: /Products/Edit/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        // POST: /Products/Edit/5
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Price,CategoryId,SupplierId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                try
                {
                    existingProduct.Code = product.Code;
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.SupplierId = product.SupplierId;


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(product.CategoryId, product.SupplierId);
            return View(product);
        }

        // GET: /Products/Delete/5
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

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasSupplies = await _context.SupplyItems.AnyAsync(si => si.ProductId == id);
            bool hasSales = await _context.SaleItems.AnyAsync(si => si.ProductId == id);

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hasSupplies || hasSales)
            {
                ViewData["ErrorMessage"] = "Невозможно удалить товар, так как по нему уже были продажи или поставки. Это нарушит историю учета.";
                return View(product);
            }

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}