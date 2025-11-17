namespace BarEscolar.Models
{
    public class MenuDay
    {
        public int Id { get; set; }
        public int menuweekid { get; set; }
        public DateTime Date { get; set; }
        public string option { get; set; } = string.Empty; //I guess para vegan or no
        public string MainDish { get; set; } = string.Empty;
        public string Soup { get; set; } = string.Empty;
        public string Dessert { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int MaxSeats { get; set; }
    }
}
