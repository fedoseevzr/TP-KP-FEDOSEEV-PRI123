using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ComputerStore.Controllers
{
    [Authorize(Roles = "Manager")]
    public class SuppliesController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Supplies
        public async Task<IActionResult> Index()
        {
            var supplies = await _context.Supplies
                .Include(s => s.Supplier)
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
            return View(supplies);
        }

        // GET: Supplies/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: Supplies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupplierId")] Supply supply)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                supply.UserId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Add(supply);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = supply.Id });
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", supply.SupplierId);
            return View(supply);
        }

        // GET: Supplies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supply = await _context.Supplies
                .Include(s => s.Supplier)
                .Include(s => s.SupplyItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supply == null) return NotFound();

            //Товары только этого поставщика
            var products = await _context.Products
                .Where(p => p.SupplierId == supply.SupplierId)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Id = p.Id,
                    Text = $"[{p.Code}] {p.Name}"
                })
                .ToListAsync();

            ViewData["ProductId"] = new SelectList(products, "Id", "Text");

            return View(supply);
        }

        // POST: Supplies/AddLineItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLineItem(int supplyId, int productId, int quantity, decimal purchasePrice)
        {
            if (quantity <= 0) return RedirectToAction(nameof(Details), new { id = supplyId });

            var supply = await _context.Supplies.FindAsync(supplyId);
            var product = await _context.Products.FindAsync(productId);

            if (supply == null || product == null) return NotFound();

            var item = new SupplyItem
            {
                SupplyId = supplyId,
                ProductId = productId,
                Quantity = quantity,
                PurchasePrice = purchasePrice
            };

            product.Quantity += quantity;
            supply.TotalAmount += (quantity * purchasePrice);

            _context.SupplyItems.Add(item);
            _context.Products.Update(product);
            _context.Supplies.Update(supply);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = supplyId });
        }
    }
}