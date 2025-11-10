namespace BarEscolar.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Orderid { get; set; }
        public int Productid { get; set; }
        public int Quantity { get; set; }
        public double Unitprice { get; set; }
        public double Subtotal { get; set; }
    }
}
