namespace BarEscolar.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Orderid { get; set; }
        public int Productid { get; set; }    // pode ser ID de Produto ou ID de MenuDay
        public bool IsMenu { get; set; }      // TRUE = menu, FALSE = produto
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Unitprice { get; set; }
        public double Subtotal { get; set; }

    }
}
