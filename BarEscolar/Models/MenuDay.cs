namespace BarEscolar.Models
{
    public class MenuDay
    {
        public int Id { get; set; }
        private int menuweekid;
        private DateTime Date;
        private string option;
        private string MainDish;
        private string Soup;
        private string Dessert;
        private string Notes;
        private int MaxSeats;
    }
}
