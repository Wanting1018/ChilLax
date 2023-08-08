namespace ChilLaxFrontEnd.ViewModels
{
    public class CAddToCartViewModel
    {
        public int txtCount { get; set; }
        public int txtFId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductDesc { get; set; } = null!;
        public int ProductPrice { get; set; }
        public string ProductImg { get; set; } = null!;
        public int ProductQuantity { get; set; }
    }
}
