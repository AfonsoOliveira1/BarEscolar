using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

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

        public AlunoController(JsonUserStore userStore, JsonMenuStore menuStore, JsonOrderStore orderStore, JsonProductStore productStore, JsonCategoryStore categoryStore, Authentication authentication)
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
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Aluno" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            ViewBag.User = user;

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

            // User's marked menus
            var userOrderItems = _orderStore.GetOrdersByUser(id)
                                            .SelectMany(o => _orderStore.GetOrderItemsByOrder(o.Id))
                                            .ToList();

            ViewBag.MarkedMenuIds = userOrderItems.Select(oi => oi.Productid).ToList();

            ViewBag.RemainingSeats = userOrderItems
                                     .GroupBy(oi => oi.Productid)
                                     .ToDictionary(g => g.Key, g => g.Count());

            return View(weeks);
        }

        // ----------------- Menus Marcados (Current week) -----------------
        public IActionResult MenusMarcados(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            // Flatten all order items of the user
            var orderItems = _orderStore.GetOrdersByUser(id)
                                        .SelectMany(o => _orderStore.GetOrderItemsByOrder(o.Id))
                                        .Where(oi => oi.Quantity > 0) // ignore canceled items
                                        .ToList();

            // Get all menus for display
            var allMenus = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).ToList();

            ViewBag.User = user;
            ViewBag.AllMenus = allMenus;

            return View(orderItems);
        }


        // ----------------- Histórico completo -----------------
        public IActionResult Historico(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            var allMenus = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).ToList();
            ViewBag.User = user;
            ViewBag.AllMenus = allMenus;

            var orderItems = _orderStore.GetOrdersByUser(id)
                                        .SelectMany(o => _orderStore.GetOrderItemsByOrder(o.Id))
                                        .ToList();

            return View(orderItems);
        }

        // ----------------- Marcar Menu -----------------
        public IActionResult Marcar(string id, int menuId)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            var menu = _menuStore.GetAllWeeks()
                                 .SelectMany(w => w.menuDays)
                                 .FirstOrDefault(m => m.Id == menuId);
            if (menu == null) return NotFound("Menu não encontrado.");

            ViewBag.User = user;
            return View(menu);
        }

        public IActionResult MarcarConfirmed(string id, int menuId)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            var allMenus = _menuStore.GetAllWeeks();
            var menu = allMenus.SelectMany(w => w.menuDays)
                               .FirstOrDefault(m => m.Id == menuId);
            if (menu == null) return NotFound("Menu não encontrado.");

            // Check if user already marked a menu for this day
            var userOrderItems = _orderStore.GetOrdersByUser(id)
                                            .SelectMany(o => _orderStore.GetOrderItemsByOrder(o.Id))
                                            .ToList();

            var alreadyMarkedForDay = userOrderItems
                                      .Select(oi => allMenus.SelectMany(w => w.menuDays)
                                                             .FirstOrDefault(m => m.Id == oi.Productid))
                                      .Any(m => m != null && m.Date.Date == menu.Date.Date);

            if (alreadyMarkedForDay)
                return BadRequest("Você já marcou uma refeição para este dia.");

            // Check cutoff time (10:00 today)
            if (menu.Date.Date == DateTime.Today && DateTime.Now.TimeOfDay > new TimeSpan(10, 0, 0))
                return BadRequest("A marcação só é permitida até às 10:00 do dia.");

            // Create or get today's order
            var existingOrder = _orderStore.GetOrdersByUser(id)
                                           .FirstOrDefault(o => o.Createdat.Date == DateTime.Today);

            if (existingOrder == null)
            {
                existingOrder = new Order
                {
                    Id = _orderStore.GetNextOrderId(),
                    Userid = user.ID,
                    Total = 0,
                    Createdat = DateTime.Now
                };
                _orderStore.AddOrder(existingOrder);
            }

            var orderItem = new OrderItem
            {
                Id = _orderStore.GetNextOrderItemId(),
                Orderid = existingOrder.Id,
                Productid = menu.Id,
                Quantity = 1,
                Unitprice = 0
            };
            _orderStore.AddOrderItem(orderItem);

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

            // Check cutoff time (09:30 today)
            if (menu != null && menu.Date.Date == DateTime.Today && DateTime.Now.TimeOfDay > new TimeSpan(9, 30, 0))
                return BadRequest("O cancelamento só é permitido até às 09:30 do dia.");

            // Mark the item as canceled instead of removing
            orderItem.Quantity = 0;

            return RedirectToAction("MenusMarcados", new { id });
        }

        // ----------------- Bar da Escola -----------------
        public IActionResult Bar(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");
            ViewBag.User = user;

            var userOrderItems = _orderStore.GetOrdersByUser(id)
                                .SelectMany(o => _orderStore.GetOrderItemsByOrder(o.Id))
                                .ToList();

            ViewBag.MarkedMenuIds = userOrderItems.Select(oi => oi.Productid).ToList();

            ViewBag.RemainingStocks = userOrderItems
                                     .GroupBy(oi => oi.Productid)
                                     .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Categorys = _categoryStore.GetAll();

            var prod = _productStore.GetAllProducts();
            return View(prod);
        }

        // ----------------- Details Produto -----------------
        public IActionResult DetailsProd(string id, int prodid)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            var prods = _productStore.GetAllProducts();
            var prod = prods.FirstOrDefault(p => p.Id == prodid);
            ViewBag.Categorys = _categoryStore.GetAll();
            return View(prod);
        }

        // ----------------- Comprar Menu -----------------
        public IActionResult Comprar(string id, int prodid)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            var prods = _productStore.GetAllProducts();
            var prod = prods.FirstOrDefault(p => p.Id == prodid);
            ViewBag.Categorys = _categoryStore.GetAll();
            return View(prod);
        }
        [HttpPost]
        public IActionResult ComprarConfirmed(string userid, int prodid)
        {
            var prods = _productStore.GetAllProducts();
            var prod = prods.FirstOrDefault(p => p.Id == prodid);
            var existingOrder = _orderStore.GetOrdersByUser(userid)
                               .FirstOrDefault(o => o.Createdat.Date == DateTime.Today);

            if (existingOrder == null)
            {
                existingOrder = new Order
                {
                    Id = _orderStore.GetNextOrderId(),
                    Userid = userid,
                    Total = 0,
                    Createdat = DateTime.Now
                };
                _orderStore.AddOrder(existingOrder);
            }

            var orderItem = new OrderItem
            {
                Id = _orderStore.GetNextOrderItemId(),
                Orderid = existingOrder.Id,
                Productid = prodid,
                Quantity = 1,
                Unitprice = 0
            };
            _orderStore.AddOrderItem(orderItem);
            return RedirectToAction("Bar", new { id = userid });
        }
    }
}
