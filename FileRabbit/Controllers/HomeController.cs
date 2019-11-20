using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace FileRabbit.PL.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            if (!Directory.Exists("C:\\FileRabbitStorage"))
                Directory.CreateDirectory("C:\\FileRabbitStorage");
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
