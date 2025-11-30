using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Http;
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
        public IActionResult Login(logindata data)
        {
            if (!ModelState.IsValid)
                return View(data);

            var authenticated = _authentication.Login(data.EmailOrUsername, data.Password);
            var user = _authentication.CurrentUser;

            if (!authenticated || user == null)
            {
                ModelState.AddModelError("", "Email ou palavra-passe incorretos.");
                return View(data);
            }

            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("UserID", user.ID);
            HttpContext.Session.SetString("UserRole", user.role.ToString());

            return user.role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin", new { id = user.ID }),
                UserRole.Funcionario => RedirectToAction("Index", "Funcionarios", new { id = user.ID }),
                UserRole.Aluno => RedirectToAction("Index", "Aluno", new { id = user.ID }),
                _ => NotFound()
            };
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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // apaga toda a sessão
            return RedirectToAction("Login");
        }
    }
}
