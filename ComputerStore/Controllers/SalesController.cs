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
    [Authorize(Roles = "Manager, Seller")]
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // Журнал продаж
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
            return View(sales);
        }

        // Создание чека
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                sale.UserId = int.Parse(userIdClaim.Value);
            }
            else
            {
                // Если вдруг ID не найден, отправляем на вход
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Add(sale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = sale.Id });
            }
            return View(sale);
        }

        // Детали продажи + подготовка данных для Select2
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null) return NotFound();

            // ПОДГОТОВКА ДАННЫХ ДЛЯ SELECT2
            var products = await _context.Products
                .Where(p => p.Quantity > 0)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Id = p.Id,
                    Text = $"[{p.Code}] {p.Name} ({p.Price} руб.)"
                })
                .ToListAsync();

            ViewData["ProductId"] = new SelectList(products, "Id", "Text");
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];

            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLineItem(int saleId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Количество должно быть больше 0.";
                return RedirectToAction(nameof(Details), new { id = saleId });
            }

            var sale = await _context.Sales.FindAsync(saleId);
            var product = await _context.Products.FindAsync(productId);

            if (sale == null || product == null) return NotFound();

            if (product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Ошибка! На складе всего {product.Quantity} шт. товара '{product.Name}'.";
                return RedirectToAction(nameof(Details), new { id = saleId });
            }

            var item = new SaleItem
            {
                SaleId = saleId,
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            };

            product.Quantity -= quantity;
            sale.TotalAmount += (quantity * item.Price);

            _context.SaleItems.Add(item);
            _context.Products.Update(product);
            _context.Sales.Update(sale);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = saleId });
        }
    }
}