using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace BarEscolar.Controllers
{
    public class AdminController : Controller
    {
        private readonly JsonMenuStore _menuStore;
        private readonly JsonProductStore _productStore;
        private readonly JsonCategoryStore _categoryStore;
        private readonly JsonUserStore _userStore;
        private readonly Authentication _auth;


        public AdminController(
            JsonMenuStore menuStore,
            JsonProductStore productStore,
            JsonCategoryStore categoryStore,
            JsonUserStore userStore,
            Authentication auth)
        {
            _menuStore = menuStore;
            _productStore = productStore;
            _categoryStore = categoryStore;
            _userStore = userStore;
            _auth = auth;
        }

        // ---------------- DASHBOARD ----------------
        public IActionResult Index(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");
            
            ViewBag.Weeks = _menuStore.GetAllWeeks();
            ViewBag.Products = _productStore.GetAllProducts();
            ViewBag.Categories = _categoryStore.GetAll();
            return View();
        }

        // ---------------- MENUWEEK & MENU DAY CRUD ----------------
        public IActionResult CreateWeek(DateTime weekstart)
        {
            var week = new MenuWeek
            {
                id = _menuStore.GetAllWeeks().Any() ? _menuStore.GetAllWeeks().Max(w => w.id) + 1 : 1,
                weekstart = weekstart.ToString("yyyy-MM-dd"),
                menuDays = new List<MenuDay>()
            };
            _menuStore.AddWeek(week);
            return RedirectToAction("Index");
        }

        public IActionResult EditWeek(int id)
        {
            var week = _menuStore.FindWeekById(id);
            if (week == null) return NotFound();
            return View(week);
        }

        [HttpPost]
        public IActionResult DeleteWeek(int id)
        {
            _menuStore.RemoveWeek(id);
            return RedirectToAction("Index");
        }

        public IActionResult CreateDay(int weekId)
        {
            var day = new MenuDay
            {
                menuweekid = weekId,
                Date = DateTime.Today
            };
            return View(day);
        }

        [HttpPost]
        public IActionResult CreateDay(int weekId,MenuDay day)
        {
            day.Id = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).Any()
                ? _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).Max(d => d.Id) + 1
                : 1;

            day.menuweekid = weekId;
            _menuStore.AddDayToWeek(weekId, day);
            return RedirectToAction("EditWeek", new { id = weekId });
        }

        public IActionResult EditDay(int dayId)
        {
            var day = _menuStore.FindDayById(dayId);
            if (day == null) return NotFound();
            return View(day);
        }

        [HttpPost]
        public IActionResult EditDay(MenuDay day)
        {
            var existing = _menuStore.FindDayById(day.Id);
            if (existing == null) return NotFound();

            existing.Date = day.Date;
            existing.option = day.option;
            existing.MainDish = day.MainDish;
            existing.Soup = day.Soup;
            existing.Dessert = day.Dessert;
            existing.Notes = day.Notes;
            existing.MaxSeats = day.MaxSeats;

            _menuStore.Save();
            return RedirectToAction("EditWeek", new { id = day.menuweekid });
        }

        [HttpPost]
        public IActionResult DeleteDay(int dayId)
        {
            var day = _menuStore.FindDayById(dayId);
            if (day == null) return NotFound();
            int weekId = day.menuweekid;
            _menuStore.RemoveDay(dayId);
            return RedirectToAction("EditWeek", new { id = weekId });
        }

        // ---------------- PRODUCT CRUD ----------------
        public IActionResult Products()
        {
            var products = _productStore.GetAllProducts();
            return View(products);
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _categoryStore.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Images");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, imageFile.FileName);
                if(filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".gif")
                    || filePath.EndsWith("jpeg"))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    product.ImagePath = imageFile.FileName;
                }
                else
                {
                    return NotFound();
                }
            }
            product.Id = _productStore.GetNextProductId();
            _productStore.Add(product);
            return RedirectToAction("Products");
        }

        public IActionResult EditProduct(int id)
        {
            var product = _productStore.FindById(id);
            if (product == null) return NotFound();

            ViewBag.Categories = _categoryStore.GetAll();
            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Images");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, imageFile.FileName);
                if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".gif")
                    || filePath.EndsWith("jpeg"))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    product.ImagePath = imageFile.FileName;
                }
                else
                {
                    return NotFound();
                }
            }
            _productStore.Update(product);
            return RedirectToAction("Products");
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            _productStore.Remove(id);
            return RedirectToAction("Products");
        }

        // ---------------- CATEGORY CRUD ----------------
        public IActionResult Categories()
        {
            var categories = _categoryStore.GetAll();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            category.Id = _categoryStore.GetAll().Any() ? _categoryStore.GetAll().Max(c => c.Id) + 1 : 1;
            _categoryStore.Add(category);
            return RedirectToAction("Categories");
        }

        public IActionResult EditCategory(int id)
        {
            var category = _categoryStore.FindById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            _categoryStore.Update(category);
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            _categoryStore.Delete(id);
            return RedirectToAction("Categories");
        }

        // ---------------- USER CRUD ----------------
        public IActionResult Users()
        {
            var users = _userStore.GetAll();
            return View("IndexUsers", users); // explicitly use IndexUsers.cshtml
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string fullName, string email, string username, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("password", "Password is required.");
                return View();
            }

            _auth.CreateUser(fullName, email, username, password, role);
            return RedirectToAction("Users");
        }

        public IActionResult EditUser(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(string id, string fullName, string email, string username, string? password, UserRole role)
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound();

            user.FullName = fullName;
            user.Email = email;
            user.UserName = username;
            user.role = role;

            if (!string.IsNullOrWhiteSpace(password))
            {
                var hasher = new PasswordHasher<Users>();
                user.passwordhash = hasher.HashPassword(user, password);
            }

            _userStore.Save();
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            _userStore.RemoveUser(id);
            return RedirectToAction("Users");
        }

        // ---------------- MENUWEEKS LIST ----------------
        public IActionResult MenuWeeks()
        {
            var weeks = _menuStore.GetAllWeeks();
            return View(weeks);
        }
    }
}
