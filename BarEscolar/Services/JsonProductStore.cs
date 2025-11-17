using System.Text.Json;
using BarEscolar.Models;

namespace BarEscolar.Services
{
    public class JsonProductStore
    {
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json");
        private List<Product> _products = new();
        private List<Category> _categories = new();

        public JsonProductStore()
        {
            if (!File.Exists(_path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                var empty = new JsonData();
                File.WriteAllText(_path, JsonSerializer.Serialize(empty, new JsonSerializerOptions { WriteIndented = true }));
            }

            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = JsonSerializer.Serialize(new JsonData(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }

            var data = JsonSerializer.Deserialize<JsonData>(json) ?? new JsonData();
            _products = data.Products;
            _categories = data.Categories;
        }

        public List<Product> GetAllProducts() => _products;
        public List<Category> GetAllCategories() => _categories;

        public Product? FindById(int id) => _products.FirstOrDefault(p => p.Id == id);
        public Category? FindCategoryById(int id) => _categories.FirstOrDefault(c => c.Id == id);

        public int GetNextProductId() => _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
        public int GetNextCategoryId() => _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1;

        public void Add(Product product)
        {
            _products.Add(product);
            Save();
        }

        public void Update(Product product)
        {
            var old = FindById(product.Id);
            if (old != null)
            {
                _products.Remove(old);
                _products.Add(product);
                Save();
            }
        }

        public void Remove(int id)
        {
            var prod = FindById(id);
            if (prod != null)
            {
                _products.Remove(prod);
                Save();
            }
        }

        public void AddCategory(Category category)
        {
            _categories.Add(category);
            Save();
        }

        public void UpdateCategory(Category category)
        {
            var old = FindCategoryById(category.Id);
            if (old != null)
            {
                _categories.Remove(old);
                _categories.Add(category);
                Save();
            }
        }

        public void RemoveCategory(int id)
        {
            var cat = FindCategoryById(id);
            if (cat != null)
            {
                _categories.Remove(cat);
                Save();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(
                new JsonData
                {
                    Products = _products,
                    Categories = _categories
                },
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(_path, json);
        }

        private class JsonData
        {
            public List<Product> Products { get; set; } = new();
            public List<Category> Categories { get; set; } = new();
        }
    }
}
