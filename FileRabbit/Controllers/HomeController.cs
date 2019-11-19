using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using FileRabbit.PL.Models;
using System.IO;
using Microsoft.AspNetCore.Diagnostics;

namespace FileRabbit.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            if (!Directory.Exists("C:\\FileRabbitStorage"))
                Directory.CreateDirectory("C:\\FileRabbitStorage");
            _logger = logger;
        }

        public IActionResult Index()
        {
            // we need the id for Start work button, which redirect to Folder/Watch or Account/Login, if id = null
            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                ErrorViewModel error = new ErrorViewModel { ErrorCode = statusCode.Value };
                switch (statusCode.Value)
                {
                    case 403:
                        error.Message = "Sorry, you don't have access to this resource.";
                        break;
                    case 404:
                        error.Message = "Sorry, the page you were looking for doesn’t exist...";
                        break;
                    case 500:
                        error.Message = "Oops... An error has occurred on the server.";
                        break;
                    default:
                        error.Message = "Oops... Something happened that we did not expect.";
                        break;
                }
                return View(error);
            }
            return View();
        }

        public IActionResult ServerError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            ErrorViewModel error = new ErrorViewModel();
            HttpContext.Response.StatusCode = (int)exception.Error.Data["Status code"];
            error.ErrorCode = HttpContext.Response.StatusCode;
            error.Message = exception.Error.Message;

            return View("Error", error);
        }
    }
}
