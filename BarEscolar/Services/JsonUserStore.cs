using System.Text.Json;
using BarEscolar.Models;
namespace BarEscolar.Services
{
    public class JsonUserStore
    {
        private readonly string m_path = Path.Combine("Data", "users.json");
        private List<Users> m_users = new();

        public JsonUserStore()
        {
            string json = File.ReadAllText(m_path);

            if (string.IsNullOrWhiteSpace(json))
            {
                m_users = new List<Users>();
                File.WriteAllText(m_path, "[]");
            }
            else
            {
                m_users = JsonSerializer.Deserialize<List<Users>>(json) ?? new List<Users>();
            }
        }

        public List<Users> GetAll() => m_users;

        public Users? FinByLogin(string login)
        {
            return m_users.FirstOrDefault(u =>
                u.UserName.Equals(login, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals(login, StringComparison.OrdinalIgnoreCase));
        }

        public Users? FindById(string id)
        {
            return m_users.FirstOrDefault(u => u.ID == id);
        }

        public void AddUser(Users user)
        {
            if (!m_users.Any(u => u.ID == user.ID))
            {
                m_users.Add(user);
                Save();
            }
        }

        public void RemoveUser(string id)
        {
            var user = m_users.FirstOrDefault(u => u.ID == id);
            if (user != null)
            {
                m_users.Remove(user);
                Save();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(m_users, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(m_path, json);
        }
    }
}
