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
            ViewBag.User = user;
            return View();
        }

        // ---------------- MENUWEEK & MENU DAY CRUD ----------------
        public IActionResult CreateWeek(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");
            ViewBag.User = user;
            return View();
        }
        [HttpPost]
        public IActionResult CreateWeek(DateTime weekstart, string id)
        {
            var week = new MenuWeek
            {
                id = _menuStore.GetAllWeeks().Any() ? _menuStore.GetAllWeeks().Max(w => w.id) + 1 : 1,
                weekstart = weekstart.ToString("yyyy-MM-dd"),
                menuDays = new List<MenuDay>()
            };
            _menuStore.AddWeek(week);
            return RedirectToAction("MenuWeeks", new {id = id});
        }
        [HttpGet]
        public IActionResult EditWeek(int weekid, string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var week = _menuStore.FindWeekById(weekid);
            if (week == null) return NotFound();
            return View(week);
        }

        [HttpPost]
        public IActionResult DeleteWeek(int weekid, string id)
        {
            _menuStore.RemoveWeek(weekid);
            return RedirectToAction("MenuWeeks", new {id = id});
        }

        public IActionResult CreateDay(int weekId, string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var day = new MenuDay
            {
                menuweekid = weekId,
                Date = DateTime.Today
            };
            return View(day);
        }

        [HttpPost]
        public IActionResult CreateDay(int weekId,MenuDay day, string id)
        {
            day.Id = _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).Any()
                ? _menuStore.GetAllWeeks().SelectMany(w => w.menuDays).Max(d => d.Id) + 1
                : 1;

            day.menuweekid = weekId;
            _menuStore.AddDayToWeek(weekId, day);
            return RedirectToAction("EditWeek", new { weekid = day.menuweekid, id = id });
        }

        public IActionResult EditDay(int dayId, string userid)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(userid);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var day = _menuStore.FindDayById(dayId);
            if (day == null) return NotFound();
            return View(day);
        }

        [HttpPost]
        public IActionResult EditDay(MenuDay day, string userid)
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
            return RedirectToAction("EditWeek", new { weekid = day.menuweekid, id = userid });
        }

        [HttpPost]
        public IActionResult DeleteDay(int dayId, string id)
        {
            var day = _menuStore.FindDayById(dayId);
            if (day == null) return NotFound();
            int weekId = day.menuweekid;
            _menuStore.RemoveDay(dayId);
            return RedirectToAction("EditWeek", new { weekid = day.menuweekid, id = id });
        }

        // ---------------- PRODUCT CRUD ----------------
        public IActionResult Products(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            ViewBag.Categories = _categoryStore.GetAll();
            var products = _productStore.GetAllProducts();
            return View(products);
        }
        public IActionResult LoadProducts(string id, string category = "", string price = "", string allergen = "")
        {
            var user = _userStore.FindById(id);
            if (user == null) return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            var prod = _productStore.GetAllProducts().AsEnumerable();

            // Filtro categoria
            if (!string.IsNullOrEmpty(category))
            {
                var cat = _categoryStore.GetAll().FirstOrDefault(c => c.Name == category);
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

        public IActionResult CreateProduct(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            ViewBag.Categories = _categoryStore.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product, IFormFile imageFile, string id)
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
            return RedirectToAction("Products", new {id = id});
        }

        public IActionResult EditProduct(string userid, int prodid)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(userid);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var product = _productStore.FindById(prodid);
            if (product == null) return NotFound();

            ViewBag.Categories = _categoryStore.GetAll();
            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product product, IFormFile imageFile, string userid)
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
            return RedirectToAction("Products", new {id = userid});
        }

        [HttpPost]
        public IActionResult DeleteProduct(string id, int prodid)
        {
            _productStore.Remove(prodid);
            return RedirectToAction("Products", new {id = id});
        }

        // ---------------- CATEGORY CRUD ----------------
        public IActionResult Categories(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var categories = _categoryStore.GetAll();
            return View(categories);
        }

        public IActionResult CreateCategory(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category, IFormFile imageFile, string id)
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
                    category.ImagePath = imageFile.FileName;
                }
                else
                {
                    return NotFound();
                }
            }
            category.Id = _categoryStore.GetAll().Any() ? _categoryStore.GetAll().Max(c => c.Id) + 1 : 1;
            _categoryStore.Add(category);
            return RedirectToAction("Categories", new {id = id});
        }

        public IActionResult EditCategory(int catid, string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");
            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var category = _categoryStore.FindById(catid);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category, IFormFile imageFile, string userid)
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
                    category.ImagePath = imageFile.FileName;
                }
                else
                {
                    return NotFound();
                }
            }
            _categoryStore.Update(category);
            return RedirectToAction("Categories", new {id = userid});
        }

        [HttpPost]
        public IActionResult DeleteCategory(int catid, string id)
        {
            _categoryStore.Delete(catid);
            return RedirectToAction("Categories", new {id = id});
        }

        // ---------------- USER CRUD ----------------
        public IActionResult Users(string userid)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(userid);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var users = _userStore.GetAll();
            return View("IndexUsers", users); // explicitly use IndexUsers.cshtml
        }

        public IActionResult CreateUser(string userid)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(userid);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string userid, string fullName, string email, string username, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("password", "Password is required.");
                return View();
            }

            _auth.CreateUser(fullName, email, username, password, role);
            return RedirectToAction("Users", new {userid = userid});
        }

        public IActionResult EditUser(string userid, string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(userid);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var user1 = _userStore.FindById(id);
            if (user1 == null) return NotFound();
            return View(user1);
        }

        [HttpPost]
        public IActionResult EditUser(string userid, string id, string fullName, string email, string username, string? password, UserRole role)
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
            return RedirectToAction("Users", new {userid = userid});
        }

        [HttpPost]
        public IActionResult DeleteUser(string userid, string id)
        {
            _userStore.RemoveUser(id);
            return RedirectToAction("Users", new {userid = userid});
        }

        // ---------------- MENUWEEKS LIST ----------------
        public IActionResult MenuWeeks(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            ViewBag.User = user;
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Admin" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            var weeks = _menuStore.GetAllWeeks();
            return View(weeks);
        }
    }
}
