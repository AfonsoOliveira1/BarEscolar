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
        //Prod CRUD
        [HttpGet]
        public IActionResult DetailsProd(string id, int prodid)
        {
            var product = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);
            if (product == null)
                return NotFound();

            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            return View(product);
        }

        [HttpGet]
        public IActionResult EditProd(string id, int prodid)
        {
            var produto = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);

            if (produto == null)
                return NotFound();

            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            ViewBag.User = user;
            ViewBag.Categories = _categoryStore.GetAll();
            return View(produto);
        }
        [HttpPost]
        public IActionResult EditProd(string userId, Product model)
        {
            var produto = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == model.Id);

            if (produto == null)
                return NotFound();
            _productStore.Update(model);
            return RedirectToAction("Index", new { ID = userId });
        }
        [HttpPost]
        public IActionResult DeleteProd(string userid, int prodid)
        {
            var produto = _productStore.GetAllProducts().FirstOrDefault(p => p.Id == prodid);
            if (produto == null)
                return NotFound();
            _productStore.Remove(prodid);
            return RedirectToAction("Index", new { ID = userid });
        }
        [HttpGet]
        public IActionResult CreateProd(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            ViewBag.Categories = _categoryStore.GetAll();
            return View();
        }
        [HttpPost]
        public IActionResult CreateProd(string userId, Product model)
        {
            model.Id = _productStore.GetNextProductId();
            _productStore.Add(model);
            return RedirectToAction("Index", new { ID = userId });
        }
        //Category CRUD -----------------------------------
        public IActionResult CreateCat(string id)
        {
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            ViewBag.Categories = _categoryStore.GetAll();
            return View();
        }
        [HttpPost]
        public IActionResult CreateCat(string userId, Category category)
        {
            _categoryStore.Add(category);
            ViewBag.Categories = _categoryStore.GetAll();
            return RedirectToAction("Index", new { ID = userId });
        }
        public IActionResult EditCat(string id, int catid)
        {
            var user = _userStore.FindById(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            ViewBag.User = user;
            var category = _categoryStore.FindById(catid);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCat(string userId, Category category)
        {
            _categoryStore.Update(category);
            return RedirectToAction("Index", new { ID = userId });
        }

        [HttpPost]
        public IActionResult DeleteCat(string id, int catid)
        {
            _categoryStore.Delete(catid);
            return RedirectToAction("Index", new { ID = id });
        }
    }
}
