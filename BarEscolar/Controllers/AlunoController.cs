using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class AlunoController : Controller
    {
        readonly Authentication _authentication;
        public IActionResult Index(int id)
        {
            ViewBag.User = 
            ViewBag.Order = Generics.order.FirstOrDefault(o => o.Userid == id);
            return View(Generics.menuDay.AsEnumerable());
        }

        public IActionResult MenusMarcados(int id)
        {
            ViewBag.User = Generics.users.FirstOrDefault(u => u.ID == id);
            var userOrders = Generics.order.Where(o => o.Userid == id).ToList();
            var orderItems = Generics.orderItem.Where(oi => userOrders.Any(o => o.Id == oi.Orderid)).ToList();
            ViewBag.OrderItems = orderItems;
            return View(userOrders);
        }

        public IActionResult Marcar(int id, int menuId)
        {
            var menu = Generics.menuDay.FirstOrDefault(u => u.Id == menuId);
            ViewBag.User = Generics.users.FirstOrDefault(u => u.ID == id);

            return View(menu);
        }

        public IActionResult MarcarConfirmed(int id, int prodId)
        {
            var order = new Order { Id = Generics.order.Count + 1, Userid = id, Total = 1, Createdat = DateTime.Now};
            var orderItem = new OrderItem { Id = Generics.orderItem.Count + 1, Orderid = order.Id, Productid = prodId, Quantity = 1, Unitprice = 1 };
            Generics.order.Add(order);
            Generics.orderItem.Add(orderItem);
            return RedirectToAction("Index", new { id = id });
        }
    }
}
