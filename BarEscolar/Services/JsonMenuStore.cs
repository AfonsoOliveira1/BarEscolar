using System.Text.Json;
using BarEscolar.Models;

namespace BarEscolar.Services
{
    public class JsonMenuStore
    {
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "menus.json");
        private List<MenuWeek> _menuWeeks = new();

        public JsonMenuStore()
        {
            if (!File.Exists(_path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, "[]");
            }

            string json = File.ReadAllText(_path);

            if (string.IsNullOrWhiteSpace(json))
            {
                _menuWeeks = new List<MenuWeek>();
                File.WriteAllText(_path, "[]");
            }
            else
            {
                _menuWeeks = JsonSerializer.Deserialize<List<MenuWeek>>(json) ?? new List<MenuWeek>();
            }
        }

        public List<MenuWeek> GetAllWeeks() => _menuWeeks;

        public MenuWeek? FindWeekById(int id) =>
            _menuWeeks.FirstOrDefault(w => w.id == id);

        public MenuDay? FindDayById(int dayId) =>
            _menuWeeks.SelectMany(w => w.menuDays).FirstOrDefault(d => d.Id == dayId);

        public int GetNextWeekId()
        {
            if (_menuWeeks.Count == 0)
                return 1;
            return _menuWeeks.Max(w => w.id) + 1;
        }

        public int GetNextDayId()
        {
            if (!_menuWeeks.Any() || !_menuWeeks.SelectMany(w => w.menuDays).Any())
                return 1;
            return _menuWeeks.SelectMany(w => w.menuDays).Max(d => d.Id) + 1;
        }

        public void AddWeek(MenuWeek week)
        {
            if (!_menuWeeks.Any(w => w.id == week.id))
            {
                _menuWeeks.Add(week);
                Save();
            }
        }

        public void UpdateWeek(MenuWeek updated)
        {
            var existing = _menuWeeks.FirstOrDefault(w => w.id == updated.id);

            if (existing != null)
            {
                existing.weekstart = updated.weekstart;
                existing.menuDays = updated.menuDays;
                Save();
            }
        }

        public void RemoveWeek(int weekId)
        {
            var week = _menuWeeks.FirstOrDefault(w => w.id == weekId);
            if (week != null)
            {
                _menuWeeks.Remove(week);
                Save();
            }
        }

        public void AddDayToWeek(int weekId, MenuDay day)
        {
            var week = _menuWeeks.FirstOrDefault(w => w.id == weekId);

            if (week != null && !week.menuDays.Any(d => d.Id == day.Id))
            {
                week.menuDays.Add(day);
                Save();
            }
        }

        public void UpdateDay(MenuDay updated)
        {
            foreach (var week in _menuWeeks)
            {
                var existing = week.menuDays.FirstOrDefault(d => d.Id == updated.Id);

                if (existing != null)
                {
                    existing.Date = updated.Date;
                    existing.MainDish = updated.MainDish;
                    existing.Soup = updated.Soup;
                    existing.Dessert = updated.Dessert;
                    existing.Notes = updated.Notes;
                    existing.option = updated.option;

                    Save();
                    return;
                }
            }
        }

        public void RemoveDay(int dayId)
        {
            foreach (var week in _menuWeeks)
            {
                var day = week.menuDays.FirstOrDefault(d => d.Id == dayId);
                if (day != null)
                {
                    week.menuDays.Remove(day);
                    Save();
                    return;
                }
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_menuWeeks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_path, json);
        }
    }
}
