using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ChilLaxFrontEnd.Models;
using System.Linq;
using ChilLaxFrontEnd.Models.DTO;
using System.Text.Json;

namespace ChilLaxFrontEnd.Controllers
{
    public class Checkout : Controller
    {
        private readonly ChilLaxContext _context;
        public Checkout(ChilLaxContext context)
        {
            _context = context;
        }

        public class EcpayApiService
        {
            private readonly HttpClient _httpClient;

            public EcpayApiService()
            {
                // 初始化 HttpClient
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5")
                };
            }

            public async Task<string> PostToEcpayApiAsync(Ecpay ecpay)
            {
                try
                {
                    // 將 ecpay 轉換成 JSON 格式
                    string ecpayJson = JsonSerializer.Serialize(ecpay);

                    // 設置 Content-Type 為 application/json
                    HttpContent httpContent = new StringContent(ecpayJson, Encoding.UTF8, "application/json");

                    // 呼叫綠界的 API
                    HttpResponseMessage response = await _httpClient.PostAsync("", httpContent);

                    // 確認請求是否成功
                    response.EnsureSuccessStatusCode();

                    // 讀取回傳的內容
                    string responseContent = await response.Content.ReadAsStringAsync();

                    return responseContent;
                }
                catch (HttpRequestException ex)
                {
                    // 請求出現異常
                    Console.WriteLine($"Http Request Exception: {ex.Message}");
                    throw;
                }
            }
        }


        public async Task<ActionResult<string>> Index()
        {
            ChilLaxContext db = new ChilLaxContext();

            //產生隨機亂數
            string guid_num = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 13);
            int oid = db.ProductOrder.Max(po => po.OrderId);
            string this_products = string.Empty;
            string orderId = "ChilLax" + $"{oid}";
            string msg = "備註欄";
            //需填入你的網址
            //string website = $"https://localhost:5000";
            string website = $"https://localhost:5000";

            //取得最新一筆訂單
            int maxOrderId = await db.ProductOrder.MaxAsync(p => p.OrderId);
            //ProductOrder? this_order = db.ProductOrders.FirstOrDefault(p => p.OrderId == maxOrderId);
            List<ProductOrderDetailDTO> productOrderDetails = await db.ProductOrder
               .Where(o => o.OrderId == maxOrderId)
               .Join(db.OrderDetail, po => po.OrderId, od => od.OrderId, (po, od) => new { ProductOrder = po, OrderDetail = od })
               .Join(db.Product, od => od.OrderDetail.ProductId, p => p.ProductId, (od, p) => new ProductOrderDetailDTO
               {
                   ProductOrder = od.ProductOrder,
                   OrderDetail = od.OrderDetail,
                   Product = p
               }).ToListAsync();

            foreach (var productOrderDetail in productOrderDetails)
            {
                this_products += $"{productOrderDetail.Product?.ProductName}/";
            }



            var order = new Dictionary<string, string>
            {
                //綠界需要的參數

                //訂單編號，測試階段為避免重複以亂數產稱
                { "MerchantTradeNo",  orderId},
                //交易時間
                { "MerchantTradeDate",  DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
                //交易金額
                { "TotalAmount",  $"{productOrderDetails.FirstOrDefault() ?.ProductOrder?.OrderTotalPrice}"},
                //交易描述
                { "TradeDesc",  $"{msg}"},
                //商品名稱
                { "ItemName",  $"{this_products}"},
                //Client端回傳付款結果網址(交易完成後須提供一隻API修改付款狀態，將未付款改成已付款)
                { "ReturnURL",  $"http://yulin.win/api/Checkout/UpdatePaymentAsync"},
                //付款完成通知回傳網址
                { "OrderResultURL", $"{website}"},
                //Client端返回特店的按鈕連結
                { "ClientRedirectURL",  $"{website}"},
                //特店編號(綠界提供測試商店編號)
                { "MerchantID",  "2000132"},
                //付款方式
                { "IgnorePayment",  "GooglePay#WebATM#CVS#BARCODE"},
                //交易類型(固定填aio)
                { "PaymentType",  "aio"},
                //預設付款方式
                { "ChoosePayment",  "ALL"},
                //CheckMacValue加密類型(固定填1)
                { "EncryptType",  "1"},
                //是否需要額外的付款資訊(Y/N)
                { "NeedExtraPaidInfo", "Y"}
            };

            //檢查碼
            order["CheckMacValue"] = GetCheckMacValue(order);
            return View(order);
        }

        public async Task<ActionResult<string>> Index2(string orderid)
        {
            ChilLaxContext db = new ChilLaxContext();

            //產生隨機亂數
            string guid_num = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 7);
            //int oid = db.ProductOrder.Max(po => po.OrderId);
            int oid = Convert.ToInt32(orderid);
            string this_products = string.Empty;
            string orderId = "ChilLax" + $"{oid}" +$"{guid_num}";
            string msg = "備註欄";
            //需填入你的網址
            //string website = $"https://localhost:5000";
            string website = $"https://localhost:5000";

            //取得最新一筆訂單
            int maxOrderId = await db.ProductOrder.MaxAsync(p => p.OrderId);
            //ProductOrder? this_order = db.ProductOrders.FirstOrDefault(p => p.OrderId == maxOrderId);
            List<ProductOrderDetailDTO> productOrderDetails = await db.ProductOrder
               .Where(o => o.OrderId == maxOrderId)
               .Join(db.OrderDetail, po => po.OrderId, od => od.OrderId, (po, od) => new { ProductOrder = po, OrderDetail = od })
               .Join(db.Product, od => od.OrderDetail.ProductId, p => p.ProductId, (od, p) => new ProductOrderDetailDTO
               {
                   ProductOrder = od.ProductOrder,
                   OrderDetail = od.OrderDetail,
                   Product = p
               }).ToListAsync();

            foreach (var productOrderDetail in productOrderDetails)
            {
                this_products += $"{productOrderDetail.Product?.ProductName}/";
            }



            var order = new Dictionary<string, string>
            {
                //綠界需要的參數

                //訂單編號，測試階段為避免重複以亂數產稱
                { "MerchantTradeNo",  orderId},
                //交易時間
                { "MerchantTradeDate",  DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
                //交易金額
                { "TotalAmount",  $"{productOrderDetails.FirstOrDefault() ?.ProductOrder?.OrderTotalPrice}"},
                //交易描述
                { "TradeDesc",  $"{msg}"},
                //商品名稱
                { "ItemName",  $"{this_products}"},
                //Client端回傳付款結果網址(交易完成後須提供一隻API修改付款狀態，將未付款改成已付款)
                { "ReturnURL",  $"http://yulin.win/api/Checkout/UpdatePaymentAsync"},
                //付款完成通知回傳網址
                { "OrderResultURL", $"{website}"},
                //Client端返回特店的按鈕連結
                { "ClientRedirectURL",  $"{website}"},
                //特店編號(綠界提供測試商店編號)
                { "MerchantID",  "2000132"},
                //付款方式
                { "IgnorePayment",  "GooglePay#WebATM#CVS#BARCODE"},
                //交易類型(固定填aio)
                { "PaymentType",  "aio"},
                //預設付款方式
                { "ChoosePayment",  "ALL"},
                //CheckMacValue加密類型(固定填1)
                { "EncryptType",  "1"},
                //是否需要額外的付款資訊(Y/N)
                { "NeedExtraPaidInfo", "Y"}
            };

            //檢查碼
            order["CheckMacValue"] = GetCheckMacValue(order);
            return View(order);
        }

        //Checkout/UpdatePayment/1
        [HttpGet]
        public async Task<string> UpdatePaymentAsync(int? id)
        {
            if (id == null || _context.ProductOrder == null)
                return "付款失敗";

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    ProductOrder productOrder = await _context.ProductOrder.FirstOrDefaultAsync(po => po.OrderId == id);

                    if (productOrder == null)
                        return "找不到該訂單";

                    //修改付款狀態
                    productOrder.OrderPayment = true;
                    _context.ProductOrder.Update(productOrder);
                    await _context.SaveChangesAsync();

                    //新增點數回饋
                    PointHistory pointHistory = new PointHistory();
                    pointHistory.ModifiedSource = "product";
                    pointHistory.MemberId = productOrder.MemberId;
                    pointHistory.PointDetailId = (productOrder.OrderId).ToString();
                    pointHistory.ModifiedAmount = (int)Math.Floor(productOrder.OrderTotalPrice / 10.0);
                    _context.PointHistory.Add(pointHistory);
                    await _context.SaveChangesAsync();

                    // 提交交易
                    await transaction.CommitAsync();

                    return "付款成功";
                }
                catch (Exception ex)
                {
                    // 發生例外時回滾交易
                    await transaction.RollbackAsync();
                    return "付款失敗：" + ex.Message;
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
