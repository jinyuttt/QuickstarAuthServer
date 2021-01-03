using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AuthServer
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
            services.AddControllersWithViews();
            services.AddIdentityServer(options =>
            {
                options.UserInteraction =
                new IdentityServer4.Configuration.UserInteractionOptions
                {
                    LoginUrl = "/account/login",
                    LogoutUrl = "/account/logout",
                   // LoginReturnUrlParameter = "returnUrl",
                    LogoutIdParameter = "logoutid"

                };
                //缓存参数处理  缓存起来提高了效率 不用每次从数据库查询
                options.Caching = new IdentityServer4.Configuration.CachingOptions
                {
                    ClientStoreExpiration = new TimeSpan(1, 0, 0),//设置Client客户端存储加载的客户端配置的数据缓存的有效时间 
                    ResourceStoreExpiration = new TimeSpan(1, 0, 0),// 设置从资源存储加载的身份和API资源配置的缓存持续时间
                    CorsExpiration = new TimeSpan(1, 0, 0)  //设置从资源存储的跨域请求数据的缓存时间
                };
                //IdentityServer支持一些端点的CORS。底层CORS实现是从ASP.NET Core提供的，因此它会自动注册在依赖注入系统中
                options.Cors = new IdentityServer4.Configuration.CorsOptions
                {
                    CorsPaths = { "" }, //支持CORS的IdentityServer中的端点。默认为发现，用户信息，令牌和撤销终结点
                    CorsPolicyName = "default", //【必备】将CORS请求评估为IdentityServer的CORS策略的名称（默认为"IdentityServer4"）。处理这个问题的策略提供者是ICorsPolicyService在依赖注入系统中注册的。如果您想定制允许连接的一组CORS原点，则建议您提供一个自定义的实现ICorsPolicyService
                    PreflightCacheDuration = new TimeSpan(1, 0, 0)//可为空的<TimeSpan>，指示要在预检Access-Control-Max-Age响应标题中使用的值。默认为空，表示在响应中没有设置缓存头
                };
            })//Ids4服务
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients())//把配置文件的Client配置资源放到内存
               .AddInMemoryApiResources(Config.GetApiResources())
               .AddInMemoryApiScopes(Config.Apis);//添加api资源
            services.AddAuthentication("oidc")
    .AddCookie("oidc", options =>
    {
        options.ExpireTimeSpan = new System.TimeSpan(0, 1, 0);
       // options.LoginPath = "/account/login";
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
    });
            // 配置cookie策略
            services.Configure<CookiePolicyOptions>(options =>
            {
                //https://docs.microsoft.com/zh-cn/aspnet/core/security/samesite?view=aspnetcore-3.1&viewFallbackFrom=aspnetcore-3
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });
            services.AddCors();

            services.AddDbContextPool<NpgsqlContext>(options =>
              {
                  options.UseNpgsql(Configuration.GetConnectionString("conn"));
              });


            //services.AddDbContext<NpgsqlContext>(options => options.UseNpgsql(Configuration.GetConnectionString("conn")));//注入DbContext
            services.AddTransient<IAdminService, AdminService>();//service注入

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
           // app.UseIdentityServer();

           
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();

            app.UseAuthorization();
           

            app.UseCookiePolicy();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
