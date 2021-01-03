using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Controllers
{
    
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IAdminService adminService;
        public HomeController(IAdminService _adminService)
        {
            adminService = _adminService;
        }

        public IActionResult Index()
        {
           
            return View();
           
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        ///// <summary>
        ///// Shows the error page
        ///// </summary>
        //public  IActionResult Error(string errorId)
        //{
        //    var vm = new ErrorViewModel();

        //    // retrieve error details from identityserver
        //    //var message = await _interaction.GetErrorContextAsync(errorId);
        //    //if (message != null)
        //    //{
        //    //    vm.Error = message;

        //    //    if (!_environment.IsDevelopment())
        //    //    {
        //    //        // only show in development
        //    //        message.ErrorDescription = null;
        //    //    }
        //    //}

        //    return View("Error", vm);
        //}
    }
}
