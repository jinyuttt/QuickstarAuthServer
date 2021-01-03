using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Client
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new HttpClient();
            var disco =await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }
            var dic = new Dictionary<string, string>();
            dic["userName"] = "admin";
            dic["password"] = "1234";
            var tokenResponse = await client.RequestTokenAsync(new TokenRequest()
            {
                Address = disco.TokenEndpoint,

                ClientId = "clientid",
                ClientSecret = "secret",
                GrantType = "Code",
                Parameters =
    {
        { "custom_parameter", "custom value"},
        { "scope", "api1" }
    }

            });

           var   response1 = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });
           var   response2 = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "clientid",
                ClientSecret = "secret",
                Scope = "api1",

                UserName = "admin",
                Password = "123",
                 
            });
            //var response4 = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            //{
            //    Address = disco.TokenEndpoint,

            //    ClientId = "client",
            //    ClientSecret = "secret",

            //    // Code = code,
            //    RedirectUri = "https://app.com/callback",

            //    // optional PKCE parameter
            //    CodeVerifier = "xyz"
            //});
            //var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest()
            //{
            //    ClientId = "clientid",
            //    ClientSecret = "secret",
            //      Parameters=dic,
            //       GrantType= "Implicit",
            //    Address = disco.TokenEndpoint,

            //});
            //if (tokenResponse.IsError)
            //{
            //    Console.WriteLine(tokenResponse.Error);
            //    return;
            //}


            //  Console.WriteLine(tokenResponse.Json);

            client = new HttpClient();
            client.SetBearerToken(response1.AccessToken);

            var response = await client.GetAsync("https://localhost:2002/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
            
                var content = await response.Content.ReadAsStringAsync();
                if (response.RequestMessage.RequestUri.ToString().Contains("https://localhost:5001/home/error"))
                {
                   var rsp=  await client.RegisterClientAsync(new DynamicClientRegistrationRequest()
                   {
                       Address = disco.TokenEndpoint,
                       ClientId = "clientid",
                       ClientSecret = "secret",
                     
                       RequestUri = new Uri("https://localhost:5001"),
                        Parameters = {
                            { "grant_type", "client_credentials" },
                     
                   },
                        Token=response1.AccessToken,

                   });
                    client.SetToken("oidc",rsp.RegistrationAccessToken);
                    response = await client.GetAsync("https://localhost:2002/api/identity");
                }
                content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
            Console.WriteLine("Hello World!");
        }
    }
}
