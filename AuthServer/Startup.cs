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
                //�����������  �������������Ч�� ����ÿ�δ����ݿ��ѯ
                options.Caching = new IdentityServer4.Configuration.CachingOptions
                {
                    ClientStoreExpiration = new TimeSpan(1, 0, 0),//����Client�ͻ��˴洢���صĿͻ������õ����ݻ������Чʱ�� 
                    ResourceStoreExpiration = new TimeSpan(1, 0, 0),// ���ô���Դ�洢���ص���ݺ�API��Դ���õĻ������ʱ��
                    CorsExpiration = new TimeSpan(1, 0, 0)  //���ô���Դ�洢�Ŀ����������ݵĻ���ʱ��
                };
                //IdentityServer֧��һЩ�˵��CORS���ײ�CORSʵ���Ǵ�ASP.NET Core�ṩ�ģ���������Զ�ע��������ע��ϵͳ��
                options.Cors = new IdentityServer4.Configuration.CorsOptions
                {
                    CorsPaths = { "" }, //֧��CORS��IdentityServer�еĶ˵㡣Ĭ��Ϊ���֣��û���Ϣ�����ƺͳ����ս��
                    CorsPolicyName = "default", //���ر�����CORS��������ΪIdentityServer��CORS���Ե����ƣ�Ĭ��Ϊ"IdentityServer4"���������������Ĳ����ṩ����ICorsPolicyService������ע��ϵͳ��ע��ġ�������붨���������ӵ�һ��CORSԭ�㣬�������ṩһ���Զ����ʵ��ICorsPolicyService
                    PreflightCacheDuration = new TimeSpan(1, 0, 0)//��Ϊ�յ�<TimeSpan>��ָʾҪ��Ԥ��Access-Control-Max-Age��Ӧ������ʹ�õ�ֵ��Ĭ��Ϊ�գ���ʾ����Ӧ��û�����û���ͷ
                };
            })//Ids4����
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients())//�������ļ���Client������Դ�ŵ��ڴ�
               .AddInMemoryApiResources(Config.GetApiResources())
               .AddInMemoryApiScopes(Config.Apis);//���api��Դ
            services.AddAuthentication("oidc")
    .AddCookie("oidc", options =>
    {
        options.ExpireTimeSpan = new System.TimeSpan(0, 1, 0);
       // options.LoginPath = "/account/login";
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
    });
            // ����cookie����
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


            //services.AddDbContext<NpgsqlContext>(options => options.UseNpgsql(Configuration.GetConnectionString("conn")));//ע��DbContext
            services.AddTransient<IAdminService, AdminService>();//serviceע��

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
