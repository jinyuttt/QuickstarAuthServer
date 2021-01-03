
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
           JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
           
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               //options.DefaultScheme = "Cookies";
                //options.DefaultChallengeScheme = "oidc"; //oidc => open id connect
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            })    
             .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                 //options.LoginPath = "/api/Identity";
                 //无权限，显示的页面
                 options.AccessDeniedPath = "/Authorization/AccessDenied";
                 options.ExpireTimeSpan = new System.TimeSpan(0, 1, 0);
                 //options.ReturnUrlParameter = "/api/Identity";
             })
             .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
             {
                 
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                 options.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                 //options.Authority = $"http://{Configuration["Identity:IP"]}:{Configuration["Identity:Port"]}";
                 options.Authority = "http://localhost:5000";
                 options.RequireHttpsMetadata = false;
                 options.ClientSecret = "secret";
                 options.ClientId = "clientid";
                 //options.Scope.Add("offline_access");
                 options.SaveTokens = true;
                 options.GetClaimsFromUserInfoEndpoint = true;
                 options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                 options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     NameClaimType = "name",
                     RoleClaimType = "role"
                 };
                 options.Scope.Add("openid");
                 options.Scope.Add("profile");
                 options.Scope.Add("api1");
                 options.Events = new OpenIdConnectEvents
                 {
                     /*
                      远程异常触发
                      在授权服务器取消登陆或者取消授权      
                      */
                     OnRemoteFailure = OAuthFailureHandler =>
                     {
                         //跳转首页
                         OAuthFailureHandler.Response.Redirect("/");
                         OAuthFailureHandler.HandleResponse();
                         return Task.FromResult(0);
                     }
                 };

             });


            // API授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api1");
                });
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        
            app.UseHttpsRedirection();

           
            app.UseRouting();

            app.UseStaticFiles();
            
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseCors("default");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
