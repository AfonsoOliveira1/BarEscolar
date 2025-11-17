namespace BarEscolar.Models
{
    public class MenuWeek
    {
        public int id { get; set; }
        public string weekstart { get; set; } = string.Empty;
        public List<MenuDay> menuDays { get; set; } = new();
    }
}
