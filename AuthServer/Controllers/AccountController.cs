using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private IAdminService _adminService;//自己写的操作数据库Admin表的service
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        public AccountController(IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IAdminService adminService)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _adminService = adminService;
        }

        /// <summary>
        /// 登录页面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// 登录post回发处理
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password, string returnUrl = null)
        {
            ViewData["returnUrl"] = returnUrl;
            Admin user = await _adminService.GetByStr(userName, password);
            if (user != null)
            {
                Microsoft.AspNetCore.Authentication.AuthenticationProperties props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1))
                };
               
                Claim nameClaim = new Claim(ClaimTypes.Name, user.UserName);
                Claim idClaim = new Claim(ClaimTypes.Sid, user.Id.ToString());
                //Claim genderClaim = new Claim(ClaimTypes.Gender, "female");
                //Claim countryClaim = new Claim(ClaimTypes.Country, "china");
              
                ClaimsIdentity id = new ClaimsIdentity("身份证");
                id.AddClaim(nameClaim);
                id.AddClaim(idClaim);
                //id.AddClaim(genderClaim);
                //id.AddClaim(countryClaim);
                //AuthenticationManagerExtensions.SignInAsync()
                ClaimsPrincipal principal = new ClaimsPrincipal(id);
                IdentityServer4.IdentityServerUser serverUser = new IdentityServer4.IdentityServerUser("身份证");
                serverUser.AdditionalClaims.Add(nameClaim);
                serverUser.AdditionalClaims.Add(idClaim);

                await HttpContext.SignInAsync(serverUser, props);
                if (returnUrl != null)
                {
                    return Redirect(returnUrl);
                }

                return View();
            }
            else
            {
                return View();
            }
        }
    }
}
