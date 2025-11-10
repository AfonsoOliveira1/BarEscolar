namespace BarEscolar.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int Userid { get; set; }
        public double Total { get; set; }
        public DateTime Createdat { get; set; }
        public bool State { get; set; }
    }
}
