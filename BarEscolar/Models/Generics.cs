namespace BarEscolar.Models
{
    public static class Generics
    {
        public static List<Users> users = new() 
        {
             new Users { ID = 1, Email = "mamadu@inete.net", FullName = "Mamadu Alves", password = "frfrfr" , role = UserRole.Aluno},
             new Users { ID = 0, Email = "admin@inete.net", FullName = "Admin User", password = "admin123" , role = UserRole.Admin},
             new Users { ID = 2, Email = "funcionario@inete.net", FullName = "Funcionario User", password = "func123" , role = UserRole.Funcionario}
        };

        public static List<Category> categories = new()
        {
            new Category { Id = 0, Name = "Sandes" },
            new Category { Id = 1, Name = "Sumos" },
            new Category { Id = 2, Name = "Snacks" }
        };

        public static List<Products> products = new()
        {
            new Products { Id = 1, Categoryid = 0,Name = "Sandes de Fiambre", Description = "Sandes de fiambre com pão fresco", Price = 2.50, Imagepath = "images/sandes_fiambre.jpg", Kcal = 250, Protein = 12, Fat = 8, Carbs = 30, Salt = 1, Allergens = 1, Stock = 50, IsActive = true },
            new Products { Id = 2, Categoryid = 1,Name = "Sumo de Laranja", Description = "Sumo natural de laranja", Price = 1.50, Imagepath = "images/sumo_laranja.jpg", Kcal = 120, Protein = 2, Fat = 0, Carbs = 28, Salt = 0, Allergens = 0, Stock = 100, IsActive = true },
            new Products { Id = 3, Categoryid = 2,Name = "Barra de Cereal", Description = "Barra de cereal com mel e frutos secos", Price = 1.00, Imagepath = "images/barra_cereal.jpg", Kcal = 180, Protein = 4, Fat = 6, Carbs = 25, Salt = 1, Allergens = 2, Stock = 75, IsActive = true }
        };
    }
}
