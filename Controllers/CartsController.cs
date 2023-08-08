using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChilLaxFrontEnd.Models;
using ChilLaxFrontEnd.Models.DTO;
using System.Text.Json;

namespace ChilLaxFrontEnd.Controllers
{
    public class CartsController : Controller
    {
        private readonly ChilLaxContext _context;

        public CartsController(ChilLaxContext context)
        {
            _context = context;
        }

        int mid = 1;

        // GET: Carts/Details
        public async Task<ActionResult<List<CartDTO>>> Details()
        {
            string json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            Console.WriteLine(json);
            Member member = JsonSerializer.Deserialize<Member>(json);
            int id =  member.MemberId ;

            if (id == null || _context.Cart == null) return NotFound();
   
            var cart = await _context.Cart
                .Include(c => c.Member)
                .Include(c => c.Product)
                .Where(c => c.MemberId == id)
                .Join(_context.Member, c => c.MemberId, m => m.MemberId,(c, m) => new {Carts = c, Members = m})
                .Join(_context.Product, c => c.Carts.ProductId, p => p.ProductId, (c, p) =>new CartDTO {
                    Cart = c.Carts,
                    Member = c.Members,
                    Product = p
                    })
                .ToListAsync();

            if (cart == null) return NotFound();

            // 將 orderSuccess 參數傳遞到視圖中
            ViewBag.orderSuccess = true;
            return View(cart);
        }

       
        // GET: Carts/Delete/5
        [HttpDelete("{id}")]
        [Route("Delete")]
        public async Task<string> Delete(int? id)
        {
            if (id == null || _context.Cart == null) return "刪除失敗";
            string json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            Console.WriteLine(json);
            Member member = JsonSerializer.Deserialize<Member>(json);
            int Mid = member.MemberId;

            Cart? cart = await _context.Cart
                .Include(c => c.Member)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.MemberId == Mid && c.ProductId == id);
            if (cart == null) return "刪除失敗";

            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }

        //GET: Carts/list
        public async Task<ActionResult<List<ProductOrderDetailDTO>>> list()
        {
            CartList cartList = new CartList();
            string? json = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            string? cartjson = HttpContext.Session.GetString(CDictionary.SK_CHECKOUT_DATA);
            cartList.MemberPick = JsonSerializer.Deserialize<MemberPick>(json);
            cartList.CartResultReq = JsonSerializer.Deserialize<CartResultReq>(cartjson);

            CartResultReq CartResultReq = JsonSerializer.Deserialize<CartResultReq>(cartjson);
            int totoPrice = 0;
            for (int i = 0; i < CartResultReq.trueCheckboxs.Length; i++)
            {
                int pid = CartResultReq.trueCheckboxs[i].pid;
                var product = _context.Product.Where(p => p.ProductId == pid);
                totoPrice += product.FirstOrDefault().ProductPrice * CartResultReq.trueCheckboxs[i].qty;
            }
            cartList.totoPrice = totoPrice;
            //for (int i = 0; i< productOrderDetailDTO)
            //productOrderDetailDTO.Member = _context.Member.Where(m => m.MemberId == Mid).FirstOrDefault();

            return View(cartList);
        }

    }
}
