using System.Text.Json;
using BarEscolar.Models;

namespace BarEscolar.Services
{
    public class JsonProductStore
    {
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json");
        private List<Product> _products = new();
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
        }

        public List<Product> GetAllProducts() => _products;
        public Product? FindById(int id) => _products.FirstOrDefault(p => p.Id == id);
        public int GetNextProductId() => _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
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

        public void Save()
        {
            var json = JsonSerializer.Serialize(
                new JsonData
                {
                    Products = _products,
                },
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(_path, json);
        }

        private class JsonData
        {
            public List<Product> Products { get; set; } = new();
        }
    }
}
