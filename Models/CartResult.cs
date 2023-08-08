namespace ChilLaxFrontEnd.Models
{
    public class CartResult
    {
        public int pid { get; set; }
        public int qty { get; set; }
    }
    public class CartResultReq
    {
        public CartResult[] trueCheckboxs { get; set; }
    }

    public class MemberPick
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberTel { get; set; }
        public string MemberAddress { get; set; }

    }

    public class CartList
    {
        public CartResultReq CartResultReq { get; set; }
        public MemberPick MemberPick { get; set; }
        public int totoPrice { get; set; }

    }
    public class ProductOrderReq
    {
        public string OrderAddress { get; set; }
        public string OrderNote { get; set; }
        public string OrderDate { get; set; }
    }
    public class ProductReq
    {
        public int productId { get; set; }
        public string txtCount { get; set; }
        public string memberId { get; set; }
    }
}
