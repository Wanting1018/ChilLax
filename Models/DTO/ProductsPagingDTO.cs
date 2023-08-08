using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChilLaxFrontEnd.Models.DTO
{

    public class CartProductItem
    {
        public List<Cart> cartList { get; set; }
        public List<Product> products { get; set; }
    }


    public class ProductsPagingDTO
    {
        public int? pageCount { get; set; }
        public int? nowpage { get; set; }
        public IEnumerable<SelectListItem> ProductSelectedList { get; set; }

        public List<string> ProdCategory { get; set; }

        public int TotalPages { get; set; }
        public List<Product> ProductsResult { get; set; }
        public List<Cart> carts { get; set; } 
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int ProductPrice { get; set; }
        public string ProductImg { get; set; } = null!;


        //public class CartListItem
        //{
        //    public List<Cart> cartList { get; set; }
        //    public List<Product> products { get; set; }
        //}

        public List<CartProductItem> CartListItem { get; set; }

    }
}
