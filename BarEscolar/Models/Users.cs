using Microsoft.AspNetCore.Identity;
using System.Data;

namespace BarEscolar.Models
{
    public class Users
    {
        public string ID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string passwordhash { get; set; } = string.Empty;
        public UserRole role { get; set; } = UserRole.Aluno;

        public Users() { }

        public Users(string fullName, string userName, string email, UserRole role)
        {
            FullName = fullName;
            UserName = userName;
            Email = email;
            this.role = role;
        }
    }
}
public enum UserRole
{
    Aluno = 0,
    Funcionario = 1,
    Admin = 2
}
