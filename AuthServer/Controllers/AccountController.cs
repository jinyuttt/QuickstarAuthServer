using AuthServer.Models;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAdminService _adminService;//自己写的操作数据库Admin表的service
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
        public  IActionResult Login(string returnUrl = null)
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
                AuthenticationProperties props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(5))
                };
               
                Claim nameClaim = new Claim(ClaimTypes.Name, user.UserName);
                Claim idClaim = new Claim(ClaimTypes.Sid, user.Id.ToString());
                Claim genderClaim = new Claim(ClaimTypes.Gender, "female");
                Claim countryClaim = new Claim(ClaimTypes.Country, "china");
              
                //ClaimsIdentity id = new ClaimsIdentity("身份证");
                //id.AddClaim(nameClaim);
                //id.AddClaim(idClaim);
                //id.AddClaim(countryClaim);
                //id.AddClaim(genderClaim);
               
              //  ClaimsPrincipal principal = new ClaimsPrincipal(id);
                IdentityServer4.IdentityServerUser serverUser = new IdentityServer4.IdentityServerUser("身份证");
                serverUser.AdditionalClaims.Add(nameClaim);
                serverUser.AdditionalClaims.Add(idClaim);
                serverUser.AdditionalClaims.Add(countryClaim);
                serverUser.AdditionalClaims.Add(genderClaim);

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

        [HttpGet]
        public IActionResult Logout(string logoutId)
        {
            
            // await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            LoggedOutViewModel logged = new LoggedOutViewModel();
            logged.LogoutId = logoutId;
            //获取客户端点击注销登录的地址
            var refererUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(refererUrl))
            {
                return Redirect(refererUrl);
            }
            return View(logged);
        }

        [HttpPost]
        public async Task<IActionResult> LogoutSub(string logoutId)
        {
            LoggedOutViewModel logged = new LoggedOutViewModel();

            logged.LogoutId = logoutId;
            #region IdentityServer4 退出登录后，默认会跳转到Config.Client配置的PostLogoutRedirectUris地址,做如下改动，则会动态的跳转到原来的地址
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync();
            if (logout != null)
            {
                logged.ClientName = logout.ClientName;
                logged.PostLogoutRedirectUri = logout.PostLogoutRedirectUri;

            }
            if (logout.PostLogoutRedirectUri != null)
            {
                return Redirect(logout.PostLogoutRedirectUri);
            }
           
            return View("LoggedOut", logged);
            #endregion
        }


        //[HttpPost]
        //public async Task<IActionResult> Logout(LoggedOutViewModel model)
        //{
        //    if (User?.Identity.IsAuthenticated == true)
        //    {
        //        // delete local authentication cookie
        //        await HttpContext.SignOutAsync();

        //        // raise the logout event
        //        await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        //    }
        //    if (model.TriggerExternalSignout)
        //    {
        //        // build a return URL so the upstream provider will redirect back
        //        // to us after the user has logged out. this allows us to then
        //        // complete our single sign-out processing.
        //        string url = Url.Action("Logout", new { logoutId = model.LogoutId });

        //        // this triggers a redirect to the external provider for sign-out
        //        return SignOut(new AuthenticationProperties { RedirectUri = url }, model.ExternalAuthenticationScheme);
        //    }

        //    return View("LoggedOut", model);
        //}

    }
}
