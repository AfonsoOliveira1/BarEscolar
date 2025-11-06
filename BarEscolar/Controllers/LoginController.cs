using BarEscolar.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = Generics.users.FirstOrDefault(u => u.Email == email && u.password == password);

            if (user == null) return View("Email ou palavra-passe incorretos.");

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
                return View("LoginSucesso", new { id = user.ID });
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
        public IActionResult Register(string email, string password, string fullname)
        {
            if (Generics.users.Any(u => u.Email == email))
            {
                ViewBag.Erro = "Esse email já está registado!";
                return View();
            }

            var user = new Users
            {
                ID = Generics.users.Count + 1,
                Email = email,
                password = password,
                FullName = fullname,
                role = UserRole.Aluno
            };

            Generics.users.Add(user);

            ViewBag.Sucesso = "Registo concluído com sucesso! Já pode iniciar sessão.";
            return View();
        }
    }
}
