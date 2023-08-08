using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChilLaxFrontEnd.Models;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics.Metrics;
using Microsoft.Build.Framework;
using ChilLaxFrontEnd.Models.DTO;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using static ChilLaxFrontEnd.Controllers.Checkout;
using ChilLaxFrontEnd.ViewModels;

namespace ChilLaxFrontEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsAPIController : ControllerBase
    {
        private readonly ChilLaxContext _context;
        public CartsAPIController(ChilLaxContext context)
        {
            _context = context;
        }

        // GET: api/CartsAPI/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<string> Delete(int id)
        {
            if (_context.Cart == null) return "刪除失敗";

            string json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            Console.WriteLine(json);
            Member member = JsonSerializer.Deserialize<Member>(json);
            int Mid = member.MemberId;

            Cart cart = await _context.Cart
                .Include(c => c.Member)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.MemberId == Mid && c.ProductId == id);

            if (cart == null)
                return "刪除失敗";

            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }


        //商品列表商品新增至購物車
        // POST: api/CartsAPI/ListCreate
        [HttpPost]
        [Route("ListCreate")]
        public async Task<string> ListCreate([FromBody] ProductReq productReq)
        {
            if (_context.Cart == null) return "新增失敗";

            string json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            Console.WriteLine(json);
            Member member = JsonSerializer.Deserialize<Member>(json);
            int Mid = member.MemberId;
            int Cartqty = Convert.ToInt32(productReq.txtCount);
            List<Cart> thisCart = _context.Cart.Where(c => c.MemberId == Mid).ToList();

            for (int i = 0; i < thisCart.Count; i++)
            {
                if (thisCart[i].ProductId == productReq.productId)
                {
                    thisCart[i].CartProductQuantity += Cartqty;
                    _context.Cart.Update(thisCart[i]);
                    _context.SaveChanges();
                    return "已有此商品，數量更新成功";
                }
            }

            Cart cart = new Cart();
            cart.MemberId = Mid;
            cart.ProductId = productReq.productId;
            cart.CartProductQuantity = Cartqty;

            _context.Cart.Add(cart);
            await _context.SaveChangesAsync();

            return "新增成功";
        }


        // 琬亭修改
        [HttpPost("Create")]
        public async Task<string> Create(CAddToCartViewModel cvm)
        {


            if (_context.Cart == null)
                return "新增失敗";

            string json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            Console.WriteLine(json);
            Member member = JsonSerializer.Deserialize<Member>(json);
            int Mid = member.MemberId;
            int Cartqty = cvm.txtCount;
            List<Cart> thisCart = _context.Cart.Where(c => c.MemberId == Mid).ToList();

            for (int i = 0; i < thisCart.Count; i++)
            {
                if (thisCart[i].ProductId == cvm.ProductId)
                {
                    thisCart[i].CartProductQuantity += Cartqty;
                    _context.Cart.Update(thisCart[i]);
                    _context.SaveChanges();
                    return "已有此商品，數量更新成功";
                }
            }

            Cart cart = new Cart();
            cart.MemberId = Mid;
            cart.ProductId = cvm.ProductId;
            cart.CartProductQuantity = Cartqty;

            _context.Cart.Add(cart);
            await _context.SaveChangesAsync();

            return "新增成功";
        }

        //POST: api/CartsAPI/SaveCartSession
        [HttpPost]
        [Route("SaveCartSession")]
        public string SaveCartSession( CartResultReq cartResultReq)
        {
            string cartsJson = JsonSerializer.Serialize(cartResultReq);
            HttpContext.Session.SetString(CDictionary.SK_CHECKOUT_DATA, cartsJson);

            return "list";
        }

        //POST: api/CartsAPI/SaveProductOrder
        [HttpPost]
        [Route("SaveProductOrder")]
        public async Task<ActionResult<string>> SaveProductOrder(ProductOrderReq por)
        {

            string memberjson = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            string cartjson = HttpContext.Session.GetString(CDictionary.SK_CHECKOUT_DATA);
            Member member = JsonSerializer.Deserialize<Member>(memberjson);
            int mid = member.MemberId;

            CartResultReq CartResultReq = JsonSerializer.Deserialize<CartResultReq>(cartjson);
            int totoPrice = 0;
            for (int i =0; i< CartResultReq.trueCheckboxs.Length; i++)
            {
                int pid = CartResultReq.trueCheckboxs[i].pid;
                var product = _context.Product.Where(p => p.ProductId == pid);
                totoPrice += product.FirstOrDefault().ProductPrice * CartResultReq.trueCheckboxs[i].qty;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //新增訂單
                    ProductOrder productOrder = new ProductOrder();
                    productOrder.MemberId = member.MemberId;
                    productOrder.OrderPayment = false;
                    productOrder.OrderTotalPrice = totoPrice;
                    productOrder.OrderDelivery = false;
                    productOrder.OrderAddress = por.OrderAddress;
                    productOrder.OrderDate = DateTime.Parse(por.OrderDate);
                    productOrder.OrderNote = por.OrderNote;
                    productOrder.OrderState = "未出貨";
                    _context.ProductOrder.Add(productOrder);
                    await _context.SaveChangesAsync();

                    //新增訂單詳細資料表
                    int orderid = _context.ProductOrder.Max(p => p.OrderId);
                    for (int i = 0; i < CartResultReq.trueCheckboxs.Length; i++)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderId = orderid;
                        orderDetail.ProductId = CartResultReq.trueCheckboxs[i].pid;
                        orderDetail.CartProductQuantity = CartResultReq.trueCheckboxs[i].qty;
                        _context.OrderDetail.Add(orderDetail);
                    }
                    await _context.SaveChangesAsync();

                    //刪除購物車內商品
                    for (int i = 0; i < CartResultReq.trueCheckboxs.Length; i++)
                    {
                        Cart cart = new Cart();
                        cart = _context.Cart.FirstOrDefault(c => c.MemberId == mid && c.ProductId == CartResultReq.trueCheckboxs[i].pid);
                        _context.Remove(cart);
                    }
                    await _context.SaveChangesAsync();


                    await transaction.CommitAsync();

                    return "";
                   
                }
                catch (Exception ex)
                {
                    // 發生例外時回滾交易
                    await transaction.RollbackAsync();
                    return "結帳失敗：" + ex.Message;
                }
            }


           
        }

        private string GetCheckMacValue(Dictionary<string, string> order)
        {
            var param = order.Keys.OrderBy(x => x).Select(key => key + "=" + order[key]).ToList();
            var checkValue = string.Join("&", param);
            //測試用的 HashKey
            var hashKey = "5294y06JbISpM5x9";
            //測試用的 HashIV
            var HashIV = "v77hoKGq4kWxNNIS";
            checkValue = $"HashKey={hashKey}" + "&" + checkValue + $"&HashIV={HashIV}";
            checkValue = HttpUtility.UrlEncode(checkValue).ToLower();
            checkValue = GetSHA256(checkValue);
            return checkValue.ToUpper();
        }
        private string GetSHA256(string value)
        {
            var result = new StringBuilder();
            var sha256 = SHA256.Create();
            var bts = Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(bts);
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        
    }
}
