using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ComputerStore.Controllers
{

    [Authorize(Roles = "Manager, Seller")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        // Показывает список всех категорий
        public async Task<IActionResult> Index()
        {
            // Получаем список категорий из БД
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // GET: /Categories/Create
        // Показывает форму для создания новой категории
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        // Обрабатывает отправку формы создания
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Перенаправляем на список
            }
            return View(category); // Если ошибка, возвращаем форму
        }

        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            
            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}