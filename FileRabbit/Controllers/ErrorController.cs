using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileRabbit.PL.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FileRabbit.PL.Controllers
{
    public class ErrorController : Controller
    {
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
            if(exception.Error.Data["Status code"] != null)
                HttpContext.Response.StatusCode = (int)exception.Error.Data["Status code"];
            error.ErrorCode = HttpContext.Response.StatusCode;
            error.Message = exception.Error.Message;

            return View("Error", error);
        }
    }
}