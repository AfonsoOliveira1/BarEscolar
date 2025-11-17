using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarEscolar.Controllers
{
    public class MenuWeekController : Controller
    {
        private readonly JsonMenuStore _menuStore;

        public MenuWeekController(JsonMenuStore menuStore)
        {
            _menuStore = menuStore;
        }

        public IActionResult Index()
        {
            var weeks = _menuStore.GetAllWeeks();
            return View(weeks);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(MenuWeek model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.id = _menuStore.GetNextWeekId();
            model.menuDays = new List<MenuDay>();

            _menuStore.AddWeek(model);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var week = _menuStore.GetAllWeeks().FirstOrDefault(w => w.id == id);
            if (week == null) return NotFound();

            return View(week);
        }

        [HttpPost]
        public IActionResult Edit(MenuWeek model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _menuStore.UpdateWeek(model);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var week = _menuStore.GetAllWeeks().FirstOrDefault(w => w.id == id);
            if (week == null) return NotFound();

            return View(week);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(int id)
        {
            _menuStore.RemoveWeek(id);
            return RedirectToAction("Index");
        }
    }
}