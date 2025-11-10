using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class LoginController : Controller
    {
        private readonly Authentication _authentication;
        public LoginController(Authentication auth)
        {
            _authentication = auth;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
           var authenticated = _authentication.Login(email, password);
           var user = _authentication.CurrentUser;

            if (authenticated == false || user == null) return View("Email ou palavra-passe incorretos.");

            if (user.role == UserRole.Admin)//Admin
            {
                return RedirectToAction("Admin", new { id = user.ID });
            }
            else if (user.role == UserRole.Funcionario)//Funcionario
            {
                return RedirectToAction("Index", "Funcionarios", new {id = user.ID});
            }
            else if (user.role == UserRole.Aluno)//Aluno
            {
                return RedirectToAction("Index", "Aluno", new { id = user.ID });
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string fullname, string email, string username, string password)
        {
            if (_authentication.FinByLogin(username) != null || _authentication.FinByLogin(email) != null)
            {
                ModelState.AddModelError("", "Este nome de utilizador ou email já está registado.");
                return View();
            }

            _authentication.CreateUser(fullname, email, username, password, UserRole.Aluno);

            TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login.";
            return RedirectToAction("Login");
        }
    }
}
