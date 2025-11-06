using System.ComponentModel.DataAnnotations;

namespace BarEscolar.Models
{
    public class Products
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Categoryid { get; set; }
        public string Imagepath { get; set; } = string.Empty;
        public int Kcal { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public int Salt { get; set; }
        public int Allergens { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        /*
        public Product(int ID, string Nome, string Descrição, double Preço, int CategoriaID, string ImagemPath, int KCAL, int Proteína, 
            int Gordura, int Carbs, int Sal, int Allergens, int Stock, bool Ativo)
        {

        }
        */
    }
}