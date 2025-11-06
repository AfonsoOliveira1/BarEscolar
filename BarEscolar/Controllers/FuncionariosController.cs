using BarEscolar.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class FuncionariosController : Controller
    {
        public IActionResult Index(int id)
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
    }
}
