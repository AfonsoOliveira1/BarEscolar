using BarEscolar.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class FuncionariosController : Controller
    {
        /*public IActionResult Index(int id)
        {
            ViewBag.User = Generics.users.FirstOrDefault(u => u.ID == id); ;
            return View(Generics.products.AsEnumerable());
        }

        public IActionResult Details(int id)
        {
            var product = Generics.products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var produto = Generics.products.FirstOrDefault(p => p.Id == id);

            if (produto == null)
                return NotFound();

            ViewBag.Categories = Generics.categories;
            return View(produto);
        }
        [HttpPost] //guardar alterações
        public IActionResult Edit(Products model)
        {
            var produto = Generics.products.FirstOrDefault(p => p.Id == model.Id);

            if (produto == null)
                return NotFound();

            produto.Name = model.Name;
            produto.Price = model.Price;
            produto.Stock = model.Stock;
            produto.Description = model.Description;
            produto.Categoryid = model.Categoryid;
            produto.Kcal = model.Kcal;
            produto.Protein = model.Protein;
            produto.Fat = model.Fat;
            produto.Carbs = model.Carbs;
            produto.Salt = model.Salt;
            produto.Allergens = model.Allergens;

            return RedirectToAction("Index");
        }*/
    }
}
