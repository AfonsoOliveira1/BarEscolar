using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BarEscolar.Controllers
{
    public class AlunoController : Controller
    {
        private readonly JsonUserStore _userStore;
        private readonly JsonMenuStore _menuStore;
        private readonly JsonOrderStore _orderStore;
        private readonly JsonProductStore _productStore;
        private readonly JsonCategoryStore _categoryStore;
        private readonly Authentication _auth;

        public AlunoController(
            JsonUserStore userStore,
            JsonMenuStore menuStore,
            JsonOrderStore orderStore,
            JsonProductStore productStore,
            JsonCategoryStore categoryStore,
            Authentication authentication)
        {
            _userStore = userStore;
            _menuStore = menuStore;
            _orderStore = orderStore;
            _productStore = productStore;
            _categoryStore = categoryStore;
            _auth = authentication;
        }

        // ----------------- Menus da Semana -----------------
        public IActionResult Index(string id, string option = "A")
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");


            var allWeeks = _menuStore.GetAllWeeks();
            var weeks = allWeeks
                .Select(week => new MenuWeek
                {
                    id = week.id,
                    weekstart = week.weekstart,
                    menuDays = week.menuDays
                        .Where(m => m.Date.Date >= DateTime.Today && m.option == option)
                        .OrderBy(m => m.Date)
                        .ToList()
                })
                .Where(w => w.menuDays.Any())
                .ToList();

            // IDs de menus marcados
            var userMenuItems = _orderStore.GetOrdersByUser(id)
                                .SelectMany(o => o.OrderItems)
                                .Where(oi => oi.IsMenu && oi.Quantity > 0)
                                .ToList();

            ViewBag.MarkedMenuIds = userMenuItems.Select(oi => oi.Productid).ToList();


            // Lugares restantes
            ViewBag.RemainingSeats = userMenuItems
                                     .GroupBy(oi => oi.Productid)
                                     .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.SelectedOption = option;
            ViewBag.Categories = _categoryStore.GetAll();

            return View(weeks);
        }

        // ----------------- Menus Marcados -----------------
        public IActionResult MenusMarcados(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var orderItems = _orderStore.GetOrdersByUser(id)
                                        .SelectMany(o => o.OrderItems)
                                        .Where(oi => oi.IsMenu && oi.Quantity > 0)
                                        .ToList();

            var allMenus = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).ToList();

            ViewBag.AllMenus = allMenus;

            return View(orderItems);
        }


        // ----------------- Histórico -----------------
        public IActionResult Historico(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var orders = _orderStore.GetOrdersByUser(id);
            var allItems = orders.SelectMany(o => o.OrderItems).ToList();

            ViewBag.AllOrders = orders;
            ViewBag.AllMenus = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).ToList();
            ViewBag.AllProducts = _productStore.GetAllProducts();

            return View(allItems);
        }

        // ----------------- Marcar Menu -----------------
        public IActionResult Marcar(string id, int menuId)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var menu = _menuStore.GetAllWeeks()
                                 .SelectMany(w => w.menuDays)
                                 .FirstOrDefault(m => m.Id == menuId);

            if (menu == null) return NotFound("Menu não encontrado.");

            return View(menu); 
        }

        [HttpPost]
        public IActionResult MarcarConfirmed(string id, int menuId)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            var menu = _menuStore.GetAllWeeks()
                                 .SelectMany(w => w.menuDays)
                                 .FirstOrDefault(m => m.Id == menuId);
            if (menu == null) return NotFound("Menu não encontrado.");

            // Pega todos os menus marcados do usuário para o mesmo dia
            var userMenuItems = _orderStore.GetOrdersByUser(id)
                                           .SelectMany(o => o.OrderItems)
                                           .Where(oi => oi.IsMenu && oi.Quantity > 0)
                                           .ToList();

            var menusSameDay = userMenuItems
                               .Select(oi => _menuStore.GetAllWeeks()
                                                       .SelectMany(w => w.menuDays)
                                                       .FirstOrDefault(m => m.Id == oi.Productid))
                               .Where(m => m != null && m.Date.Date == menu.Date.Date)
                               .ToList();

            if (menusSameDay.Any())
            {
                // Verifica se o tipo é diferente
                if (menusSameDay.Any(m => m.option != menu.option))
                {
                    return BadRequest("Não é possível marcar menus vegan e não-vegan no mesmo dia.");
                }

                return BadRequest("Você já marcou um menu para este dia.");
            }

            // Checagem de horário (até 10:00)
            if (menu.Date.Date == DateTime.Today && DateTime.Now.TimeOfDay > new TimeSpan(10, 0, 0))
                return BadRequest("A marcação só é permitida até às 10:00 do dia.");

            // Criar ou obter order do dia — type Menu
            var existingOrder = _orderStore.GetOrdersByUser(id)
                                           .FirstOrDefault(o => o.Createdat.Date == DateTime.Today && o.Type == "Menu");

            if (existingOrder == null)
            {
                existingOrder = new Order
                {
                    Id = _orderStore.GetNextOrderId(),
                    Userid = user.ID,
                    Createdat = DateTime.Now,
                    Total = 0,
                    Type = "Menu"
                };
                _orderStore.AddOrder(existingOrder);
            }

            _orderStore.MarkMenu(id, menuId);

            return RedirectToAction("Index", new { id = user.ID });
        }


        // ----------------- Cancelar Marcação -----------------
        [HttpPost]
        public IActionResult CancelarMarcacao(string id, int orderItemId)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            var orderItem = _orderStore.GetOrderItemById(orderItemId);
            if (orderItem == null) return NotFound("Marcação não encontrada.");

            var menu = _menuStore.GetAllWeeks()
                                 .SelectMany(w => w.menuDays)
                                 .FirstOrDefault(m => m.Id == orderItem.Productid);

            if (menu != null && menu.Date.Date == DateTime.Today && DateTime.Now.TimeOfDay > new TimeSpan(9, 30, 0))
                return BadRequest("O cancelamento só é permitido até às 09:30 do dia.");

            orderItem.Quantity = 0;

            return RedirectToAction("MenusMarcados", new { id });
        }

        // ----------------- Bar da Escola -----------------
        public IActionResult Bar(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var userOrderItems = _orderStore.GetOrdersByUser(id)
                                            .Where(o => o.Type == "Produto")
                                            .SelectMany(o => o.OrderItems)
                                            .ToList();

            ViewBag.MarkedMenuIds = userOrderItems.Select(oi => oi.Productid).ToList();

            ViewBag.RemainingStocks = userOrderItems
                                     .GroupBy(oi => oi.Productid)
                                     .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Categories = _categoryStore.GetAll();
            var prod = _productStore.GetAllProducts().AsEnumerable();
            return View(prod);
        }

        public IActionResult LoadProducts(string id, string category = "", string price = "", string allergen = "")
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var prod = _productStore.GetAllProducts().AsEnumerable();

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedPrice = price;

            // Filtro categoria
            if (!string.IsNullOrEmpty(category))
            {
                var cat = _categoryStore.GetAll().FirstOrDefault(c => c.Name == category);
                var filteredProds = _productStore.GetAllProducts()
                                    .Where(p => p.CategoryId == cat?.Id)
                                    .ToList();
                prod = filteredProds;
                if (cat != null)
                    prod = prod.Where(p => p.CategoryId == cat.Id);
            }
            // Filtro alergénio
            if (!string.IsNullOrEmpty(allergen))
                prod = prod.Where(p => !string.IsNullOrEmpty(p.Allergens) && p.Allergens.Contains(allergen, StringComparison.OrdinalIgnoreCase));

            // Filtro preço
            if (price == "low")
                prod = prod.OrderBy(p => p.Price);
            else if (price == "high")
                prod = prod.OrderByDescending(p => p.Price);

            ViewBag.Categories = _categoryStore.GetAll();

            return PartialView("_ProductList", prod);
        }
        public IActionResult Comprar(string id, int prodid)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var prod = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);
            ViewBag.Categorys = _categoryStore.GetAll();
            return View(prod);
        }

        [HttpPost]
        public IActionResult ComprarConfirmed(string userid, int prodid)
        {
            var product = _productStore.FindById(prodid);
            if (product == null || product.Stock <= 0)
                return BadRequest("Produto indisponível.");

            // Obter order do dia apenas do tipo Produto
            var existingOrder = _orderStore.GetOrdersByUser(userid)
                                           .FirstOrDefault(o => o.Createdat.Date == DateTime.Today && o.Type == "Produto");

            if (existingOrder == null)
            {
                existingOrder = new Order
                {
                    Id = _orderStore.GetNextOrderId(),
                    Userid = userid,
                    Total = 0,
                    Createdat = DateTime.Now,
                    Type = "Produto"
                };
                _orderStore.AddOrder(existingOrder);
            }

            var orderItem = new OrderItem
            {
                Id = _orderStore.GetNextOrderItemId(),
                Orderid = existingOrder.Id,
                Productid = prodid,
                Quantity = 1,
                Unitprice = product.Price,
                Subtotal = product.Price
            };
            _orderStore.AddOrderItem(orderItem);

            product.Stock -= 1;
            _productStore.Update(product);

            return RedirectToAction("Bar", new { id = userid });
        }
    }
}
