using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using ComputerStore.Controllers;
using ComputerStore.Data;
using ComputerStore.Models;

namespace Tests
{
    public class AllControllerTests
    {
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        private ControllerContext GetManagerContext(int userId = 10)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Manager"),
                new Claim("UserId", userId.ToString())
            }, "mock"));

            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(_ => _.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(mockAuthService.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext { User = user, RequestServices = serviceProvider };

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private T SetupControllerEssentials<T>(T controller) where T : Controller
        {
            controller.TempData = new Mock<ITempDataDictionary>().Object;

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext c) => $"/{c.Controller}/{c.Action}");
            controller.Url = mockUrlHelper.Object;

            return controller;
        }

        // ТЕСТЫ ProductsController

        [Fact]
        public async Task Product_Create_Post_ValidData_AddsProductAndRedirects()
        {
            using var context = GetDatabaseContext();
            var controller = SetupControllerEssentials(new ProductsController(context)); 

            context.Categories.Add(new Category { Id = 1, Name = "ТестКатегория" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "ТестПоставщик",
                INN = "1234567890",
                Phone = "+71234567890"
            });
            await context.SaveChangesAsync();

            var newProduct = new Product
            {
                Name = "Новый Процессор",
                Code = "CPU-001",
                Price = 25000m,
                Quantity = 0,
                CategoryId = 1,
                SupplierId = 1
            };

            var result = await controller.Create(newProduct);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, context.Products.Count());
        }

        [Fact]
        public async Task Product_Edit_Post_UpdatesPrice_ButKeepsQuantity()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Id = 1, Name = "ТестКатегория" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "ТестПоставщик",
                INN = "1234567890",
                Phone = "+71234567890"
            });

            var existingProduct = new Product
            {
                Id = 1,
                Name = "Старый",
                Code = "1",
                Price = 100m,
                Quantity = 10,
                CategoryId = 1,
                SupplierId = 1
            };
            context.Products.Add(existingProduct);
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new ProductsController(context)); 

            var updatedProduct = new Product
            {
                Id = 1,
                Name = "Обновленный",
                Code = "100",
                Price = 200m,
                Quantity = 500, 
                CategoryId = 1,
                SupplierId = 1
            };

            var result = await controller.Edit(1, updatedProduct);
            Assert.IsType<RedirectToActionResult>(result);
            var productInDb = await context.Products.FindAsync(1);
            Assert.Equal(200m, productInDb.Price);
            Assert.Equal("Обновленный", productInDb.Name);
            Assert.Equal(10, productInDb.Quantity);
        }

        // Тесты SalesController

        [Fact]
        public async Task Sales_Create_Post_ValidSale_DecreasesProductStock()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Id = 1, Name = "К" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "П",
                INN = "1234567890",
                Phone = "+7"
            });
            var product = new Product { Id = 1, Name = "GPU", Price = 1000m, Quantity = 5, Code = "GPU1", CategoryId = 1, SupplierId = 1 };
            var user = new User { Id = 10, Login = "Seller", Password = "123", Role = "Seller" };
            context.Products.Add(product);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new SalesController(context)); 
            controller.ControllerContext = GetManagerContext(10);

            var sale = new Sale { Date = DateTime.Now };
            var createResult = await controller.Create(sale);

            var redirectToDetails = Assert.IsType<RedirectToActionResult>(createResult);
            var saleId = (int)redirectToDetails.RouteValues["id"];

            var productBeforeAdd = await context.Products.FindAsync(1);
            Assert.Equal(5, productBeforeAdd.Quantity);

            var addItemResult = await controller.AddLineItem(saleId, 1, 2);

            Assert.IsType<RedirectToActionResult>(addItemResult);
            var productInDb = await context.Products.FindAsync(1);

            Assert.Equal(3, productInDb.Quantity);
        }

        [Fact]
        public async Task Sales_Create_Post_NotEnoughStock_ReturnsError()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Id = 1, Name = "К" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "П",
                INN = "1234567890",
                Phone = "+7"
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "LowStock",
                Price = 10m,
                Quantity = 1,
                CategoryId = 1,
                SupplierId = 1
            });

            var user = new User { Id = 10, Login = "Seller", Password = "123", Role = "Seller" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new SalesController(context));
            controller.ControllerContext = GetManagerContext(10);

            var sale = new Sale { Date = DateTime.Now, UserId = 10 };
            var createResult = await controller.Create(sale);
            var redirectToDetails = Assert.IsType<RedirectToActionResult>(createResult);
            var saleId = (int)redirectToDetails.RouteValues["id"];

            var addItemResult = await controller.AddLineItem(saleId, 1, 5); 

            Assert.IsType<RedirectToActionResult>(addItemResult);

            var productInDb = await context.Products.FirstAsync();
            Assert.Equal(1, productInDb.Quantity); 
        }

        // Тесты SuppliesController 

        [Fact]
        public async Task Supplies_Create_Post_ValidSupply_IncreasesProductStock()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Id = 1, Name = "К" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "Поставщик",
                INN = "1234567890",
                Phone = "+71234567890"
            });
            context.Products.Add(new Product { Id = 1, Name = "Empty", Price = 10m, Quantity = 0, Code = "E1", CategoryId = 1, SupplierId = 1 });

            var user = new User { Id = 10, Login = "Manager", Password = "123", Role = "Manager" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new SuppliesController(context)); 
            controller.ControllerContext = GetManagerContext(10);

            var supply = new Supply { SupplierId = 1 };
            var createResult = await controller.Create(supply);

            var redirectToDetails = Assert.IsType<RedirectToActionResult>(createResult);
            var supplyId = (int)redirectToDetails.RouteValues["id"];

            var addItemResult = await controller.AddLineItem(supplyId, 1, 100, 5m);

            Assert.IsType<RedirectToActionResult>(addItemResult);
            var productInDb = await context.Products.FindAsync(1);

            Assert.Equal(100, productInDb.Quantity);
        }

        // Тесты DebitsController

        [Fact]
        public async Task Debits_Create_Post_ValidDebit_DecreasesStock()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Id = 1, Name = "К" });
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Name = "П",
                INN = "1234567890",
                Phone = "+7"
            });
            context.Products.Add(new Product { Id = 1, Name = "Broken", Price = 10m, Quantity = 10, Code = "B1", CategoryId = 1, SupplierId = 1 });
            var user = new User { Id = 10, Login = "Manager", Password = "123", Role = "Manager" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new DebitsController(context)); 
            controller.ControllerContext = GetManagerContext(10);

            var debit = new Debit
            {
                ProductId = 1,
                Quantity = 1,
                Reason = "Брак"
            };

            var result = await controller.Create(debit);

            var productInDb = await context.Products.FindAsync(1);

            Assert.Equal(9, productInDb.Quantity);
            Assert.Equal(1, context.Debits.Count());
        }

        // Тесты CategoriesController

        [Fact]
        public async Task Categories_Index_ReturnsViewWithCategories()
        {
            using var context = GetDatabaseContext();
            context.Categories.Add(new Category { Name = "Видеокарты" });
            context.Categories.Add(new Category { Name = "Процессоры" });
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new CategoriesController(context));

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Category>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        // Тесты ReportsController

        [Fact]
        public async Task Reports_Sales_WithDates_ReturnsFilteredReport()
        {
            using var context = GetDatabaseContext();

            var user = new User { Id = 10, Login = "Manager", Password = "123", Role = "Manager" };
            context.Users.Add(user);

            context.Categories.Add(new Category { Id = 1, Name = "К" });
            context.Suppliers.Add(new Supplier { Id = 1, Name = "П", INN = "123", Phone = "+7" });
            var product = new Product { Id = 1, Name = "P", Price = 10m, Quantity = 10, Code = "P1", CategoryId = 1, SupplierId = 1 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var targetDate = new DateTime(2025, 1, 15);
            var outsideDate = new DateTime(2025, 2, 1);

            context.Sales.Add(new Sale { Id = 1, Date = targetDate, UserId = 10, TotalAmount = 100m });
            context.Sales.Add(new Sale { Id = 2, Date = outsideDate, UserId = 10, TotalAmount = 50m });
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new ReportsController(context)); 

            var result = await controller.Sales(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

            var viewResult = Assert.IsType<ViewResult>(result);
        }


        // Тесты AccountController

        [Fact]
        public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
        {
            using var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Login = "admin", Password = "password123", Role = "Manager" });
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new AccountController(context));
            controller.ControllerContext = GetManagerContext(1);

            var loginModel = new LoginViewModel { Login = "admin", Password = "wrong_password" };

            var result = await controller.Login(loginModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Login_Post_NonExistentUser_ReturnsViewWithError()
        {
            using var context = GetDatabaseContext();
            var controller = SetupControllerEssentials(new AccountController(context)); 
            controller.ControllerContext = GetManagerContext(1);

            var loginModel = new LoginViewModel { Login = "ghost_user", Password = "123" };

            var result = await controller.Login(loginModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_RedirectsToSalesIndex()
        {
            using var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Login = "admin", Password = "password123", Role = "Manager" });
            await context.SaveChangesAsync();

            var controller = SetupControllerEssentials(new AccountController(context)); 
            controller.ControllerContext = GetManagerContext(1);

            var loginModel = new LoginViewModel { Login = "admin", Password = "password123" };

            var result = await controller.Login(loginModel);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Sales", redirectToActionResult.ControllerName);
        }
    }
}