using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; 

namespace ComputerStore.Controllers
{
    [Authorize(Roles = "Manager")]
    public class DebitsController : Controller
    {
        private readonly AppDbContext _context;

        public DebitsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Debits
        // Просмотр всех списаний
        public async Task<IActionResult> Index()
        {
            // Загружаем связанные данные (Товар и Пользователь)
            var debits = _context.Debits
                .Include(d => d.Product)
                .Include(d => d.User)
                .OrderByDescending(d => d.Date);
            return View(await debits.ToListAsync());
        }

        // GET: Debits/Create?productId=5
        // Форма для нового списания
        public async Task<IActionResult> Create(int? productId)
        {
            if (productId == null)
            {
                // Если ID товара не передан, отправляем на выбор товара
                return RedirectToAction("Index", "Products");
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            // Создаем временную модель для передачи в представление
            var model = new Debit { ProductId = product.Id, Product = product };
            ViewData["ProductName"] = product.Name;
            ViewData["CurrentQuantity"] = product.Quantity;

            return View(model);
        }

        // POST: Debits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity,Reason")] Debit debit)
        {
            var product = await _context.Products.FindAsync(debit.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            // Валидация остатков
            if (debit.Quantity <= 0 || debit.Quantity > product.Quantity)
            {
                ModelState.AddModelError("Quantity", $"Некорректное количество. Доступно: {product.Quantity}");
            }

            var userIdString = User.FindFirstValue("UserId"); 
            int userId;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out userId))
            {
                // Если ID пользователя не найден, добавляем ошибку в ModelState
                ModelState.AddModelError(string.Empty, "Ошибка авторизации: ID пользователя не найден в сессии.");
            }
            else
            {
                // Устанавливаем ID пользователя только если он успешно извлечен
                debit.UserId = userId;
            }

            if (ModelState.IsValid)
            {
                // Обновляем остаток товара
                product.Quantity -= debit.Quantity;
                _context.Update(product);

                // Создаем запись о списании
                debit.Date = DateTime.Now;

                _context.Add(debit);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Products");
            }

            // Если модель не прошла валидацию
            ViewData["ProductName"] = product.Name;
            ViewData["CurrentQuantity"] = product.Quantity;
            return View(debit);
        }
    }
}