using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BarEscolar.Controllers
{
    public class FuncionariosController : Controller
    {
        private readonly JsonUserStore _userStore;
        private readonly JsonMenuStore _menuStore;
        private readonly JsonOrderStore _orderStore;
        private readonly JsonProductStore _productStore;
        private readonly JsonCategoryStore _categoryStore;
        private readonly Authentication _auth;

        public FuncionariosController(JsonUserStore userStore, JsonMenuStore menuStore, JsonOrderStore orderStore, JsonProductStore productStore, JsonCategoryStore categoryStore, Authentication authentication)
        {
            _userStore = userStore;
            _menuStore = menuStore;
            _orderStore = orderStore;
            _productStore = productStore;
            _categoryStore = categoryStore;
            _auth = authentication;
        }
        public IActionResult Index(string id)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (userName == null || userRole != "Funcionario" || userId != user.ID)
                return RedirectToAction("Login", "Login");

            ViewBag.User = user;
            ViewBag.Products = _productStore.GetAllProducts();
            ViewBag.Categories = _categoryStore.GetAll();
            return View(_productStore.GetAllProducts());
        }

        public IActionResult Details(int prodid)
        {
            var product = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);
            if (product == null)
                return NotFound();
            
            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(string id, int prodid)
        {
            var produto = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);

            if (produto == null)
                return NotFound();

            ViewBag.Categories = _categoryStore.GetAll();
            return View(produto);
        }
        [HttpPost] //guardar alterações
        public IActionResult Edit(Product model, string id)
        {
            var produto = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == model.Id);

            if (produto == null)
                return NotFound();

            produto.Name = model.Name;
            produto.Price = model.Price;
            produto.Stock = model.Stock;
            produto.Description = model.Description;
            produto.CategoryId = model.CategoryId;
            produto.Kcal = model.Kcal;
            produto.Protein = model.Protein;
            produto.Fat = model.Fat;
            produto.Carbs = model.Carbs;
            produto.Salt = model.Salt;
            produto.Allergens = model.Allergens;

            return RedirectToAction("Index");
        }
    }
}
