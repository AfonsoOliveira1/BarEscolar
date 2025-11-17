using BarEscolar.Models;
using System.Text.Json;

namespace BarEscolar.Services
{
    public class JsonCategoryStore
    {
        private readonly string _path = Path.Combine("Data", "categories.json");
        private List<Category> _categories;
        private static readonly object _lock = new();

        public JsonCategoryStore()
        {
            // Ensure the directory exists
            if (!File.Exists(_path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, "[]"); // initialize empty JSON array
            }

            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = "[]"; // fallback if file is empty
                File.WriteAllText(_path, json);
            }

            _categories = JsonSerializer.Deserialize<List<Category>>(json) ?? new List<Category>();
        }

        public List<Category> GetAll() => _categories;

        public Category? FindById(int id) => _categories.FirstOrDefault(c => c.Id == id);

        public void Add(Category category)
        {
            category.Id = _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1;
            _categories.Add(category);
            Save();
        }

        public void Update(Category category)
        {
            var index = _categories.FindIndex(c => c.Id == category.Id);
            if (index == -1) return;

            _categories[index] = category;
            Save();
        }

        public void Delete(int id)
        {
            _categories.RemoveAll(c => c.Id == id);
            Save();
        }

        public void Save()
        {
            lock (_lock)
            {
                File.WriteAllText(
                    _path,
                    JsonSerializer.Serialize(_categories, new JsonSerializerOptions { WriteIndented = true })
                );
            }
        }
    }
}
