using ChilLaxFrontEnd.Models;
using System.ComponentModel;

namespace ChilLaxFrontEnd.Models.DTO
{
    public class ProductOrderDetailDTO
    {
        //ChilLaxContext db = new ChilLaxContext();

        public ProductOrder? ProductOrder { get; set; }
        public OrderDetail? OrderDetail { get; set; }
        public Product? Product { get; set; }
        public Member? Member { get; set; }
        public Cart? Cart { get; set; }

    }

    public class MemberOrder
    {
        public List<ProductOrder> ProductOrder { get; set; }
        public List<OrderDetail> orderDetails { get; set; }
        public Product Product { get; set; }
        public Member Member { get; set; }
    }
}