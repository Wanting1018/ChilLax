using ChilLaxFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChilLaxFrontEnd.Controllers
{
    public class CustomerServiceController : Controller
    {
        ChilLaxContext db = new ChilLaxContext();
        public IActionResult Index()
        {
            
            return View();
        }
    }
}
