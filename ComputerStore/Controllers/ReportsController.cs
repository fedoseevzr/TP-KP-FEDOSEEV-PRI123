using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerStore.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // ОТЧЕТ ПО ОСТАТКАМ
        public async Task<IActionResult> Inventory()
        {
            // Загружаем товары с категориями и поставщиками
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // ОТЧЕТ ПО ПРОДАЖАМ 
        public async Task<IActionResult> Sales(DateTime? start, DateTime? end)
        {
            // Если даты не указаны, берем начало и конец текущего месяца
            var startDate = start ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = end ?? DateTime.Now;

            //Что-бы endDate захватил весь конец дня (23:59:59),мы берем начало следующего дня и используем строгое неравенство <
            var filterEndDate = endDate.Date.AddDays(1);

            var sales = await _context.Sales
                .Include(s => s.User)
                .Where(s => s.Date >= startDate && s.Date < filterEndDate)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var model = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                Sales = sales,
                TotalRevenue = sales.Sum(s => s.TotalAmount)
            };

            return View(model);
        }
    }
}