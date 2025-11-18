namespace BarEscolar.Models
{
    public class Product
    {
        public string Imagepath { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get;
            set;
        }

        public int CategoryId { get; set; }

        // valores nutricionais por porção
        public int Kcal { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public int Salt { get; set; }
        public string Allergens { get; set; }

        public int Stock { get; set; }

        // Product active/inactive state
        public bool IsActive { get; set; }
    }
}