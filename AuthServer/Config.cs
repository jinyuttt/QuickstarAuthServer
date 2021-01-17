using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace AuthServer
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                
                // OpenID Connect隐式流客户端（MVC）
                new Client
                {
                    ClientId = "clientid",
                    ClientName = "API",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    RequireConsent=false,//如果不需要显示否同意授权 页面 这里就设置为false
                     // RedirectUris = { "https://localhost:44371/signin-oidc" },//登录成功后返回的客户端地址
                   PostLogoutRedirectUris = { "http://localhost:2002/signout-callback-oidc" },//注销登录后返回的客户端地址
                    ClientSecrets = { new Secret("secret".Sha256()) },
                     RequirePkce=false,
                     RedirectUris={ "https://localhost:2002/signin-oidc" },
                      
                    //  AccessTokenLifetime=60,
                        AllowOfflineAccess = true,
                         AlwaysIncludeUserClaimsInIdToken = true,
                        //  AccessTokenType = AccessTokenType.Reference,
                          AllowAccessTokensViaBrowser=true,//允许浏览器传输
                    AllowedScopes =//下面这两个必须要加吧 不太明白啥意思
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
             return new List<ApiResource>
            {                new ApiResource("api1", "My API")
            };

        }
        /// <summary>
        /// 新版本
        /// </summary>
        public static IEnumerable<ApiScope> Apis =>
            new List<ApiScope>
            {
                new ApiScope("api1", "my api1"),
            };
    }
}
