using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class IdentityController : ControllerBase
    {
        //[HttpGet]
        //public IActionResult Get()
        //{
        //    return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        //}
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

       
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
          
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            //await HttpContext.SignOutAsync("Cookies");
            //await HttpContext.SignOutAsync("oidc");
        }
    }
}
