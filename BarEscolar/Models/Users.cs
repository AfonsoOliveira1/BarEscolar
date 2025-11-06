using Microsoft.AspNetCore.Identity;

namespace BarEscolar.Models
{
    public class Users 
    {
        public int ID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public UserRole role  { get; set; } = UserRole.Aluno;
    }
}
public enum UserRole
{
    Aluno = 0,
    Funcionario = 1,
    Admin = 2
}