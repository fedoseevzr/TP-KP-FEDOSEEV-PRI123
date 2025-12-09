using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ComputerStore.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {

            context.Database.Migrate();

            if (context.Users.Any())
            {
                return;
            }

            //Создаем Менеджера 
            var oleg = new User
            {
                Login = "Олег",
                // Пароль
                Password = "123456",
                Role = "Manager"
            };

            //Создаем Продавца
            var grisha = new User
            {
                Login = "Гриша",
                // Пароль
                Password = "654321",
                Role = "Seller"
            };

            // Добавляем новых пользователей в контекст
            context.Users.AddRange(oleg, grisha);

            // Сохраняем изменения в базе данных
            context.SaveChanges();

        }
    }
}