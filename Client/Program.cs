using IdentityModel.Client;
using IdentityModel.OidcClient;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
namespace Client
{
    class Program
    {
        static OidcClient _oidcClient;
        static HttpClient _apiClient = new HttpClient { BaseAddress = new Uri("https://localhost:2002/api/identity") };
        static async System.Threading.Tasks.Task Main(string[] args)
        {
           await SignIn();
          // await NewMethod();
            Console.WriteLine("Hello World!");
        }

        private static async Task NewMethod()
        {
            var client = new HttpClient();


            HttpContent req11 = new FormUrlEncodedContent(
             new[]{
                     new KeyValuePair<string,string>("username","admin"),
                     new KeyValuePair<string, string>("password","123"),
             });



            var httpclientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
                UseCookies = true
            };

            client = new HttpClient(httpclientHandler);



            var response = await client.GetAsync("https://localhost:2002/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {

                var content = await response.Content.ReadAsStringAsync();
                string str = response.RequestMessage.RequestUri.Query;
                string req = response.RequestMessage.RequestUri.AbsoluteUri.Replace(str, "");


                HttpContent req1 = new FormUrlEncodedContent(
               new[]{
                     new KeyValuePair<string,string>("username","admin"),
                     new KeyValuePair<string, string>("password","123"),
                    new KeyValuePair<string, string>("returnUrl","https://localhost:2002/api/identity"),


               });
                httpclientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
                    UseCookies = true
                };

                var rsp = await client.PostAsync(req, req1);


                content = await rsp.Content.ReadAsStringAsync();
                XmlDocument document = new XmlDocument();
                document.LoadXml(content);
                var id_tolen = document.DocumentElement.SelectNodes("//*[@name=\"id_token\"]");
                var code = document.DocumentElement.SelectNodes("//*[@name=\"code\"]");
                string token = "";
                string codev = "";
                if (id_tolen.Count > 0)
                {
                    token = id_tolen[0].Attributes["value"].Value;
                }
                if (code.Count > 0)
                {
                    codev = code[0].Attributes["value"].Value;
                }

                var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
                var response1 = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest()
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "clientid",
                    Code = codev,
                    ClientSecret = "secret",
                    RedirectUri = "https://localhost:2002/signin-oidc",
                    GrantType = "authorization_code",
                });

                var token11 = response1.AccessToken;
                //var Infos = new Dictionary<string, string>()
                //      {
                //          {"AccessToken", token11 },
                //          {"IdToken", token },
                //          {"RefreshToken", response1.RefreshToken },
                //          {"Code", codev } //code 是空 因为code 是一次性的
                //      };
                client.DefaultRequestHeaders.Clear();
                client.SetToken("id_token", token);
                client.SetToken("refresh_token", response1.RefreshToken);
                //  client.SetToken("access_token", response1.AccessToken);
                client.SetBearerToken(token11);
                var nn = await client.GetAsync("https://localhost:2002/api/identity");

                content = await nn.Content.ReadAsStringAsync();

                Console.WriteLine(content);


            }
        }

        private static async Task SignIn()
        {
            // create a redirect URI using an available port on the loopback address.
            // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
            var browser = new SystemBrowser();
            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = "https://localhost:5001",
                ClientId = "clientid",
                RedirectUri = "https://localhost:2002/signin-oidc",
                Scope = "offline_access openid profile api1 ",
                FilterClaims = false,
                ClientSecret = "secret",
                Policy = new Policy()
                {
                    Discovery = new DiscoveryPolicy()
                    { RequireKeySet = false }
                },
            
                Browser = browser,

               
                //RefreshTokenInnerHttpHandler = new HttpClientHandler(),
               
            };

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            object p = options.LoggerFactory.AddSerilog(serilog);

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest()
            {
                 FrontChannel=new FrontChannelParameters()
                 {
                     Extra = new Parameters(new[]{
                     new KeyValuePair<string,string>("username","admin"),
                    new KeyValuePair<string, string>("password","123"),
                    new KeyValuePair<string, string>("returnUrl","https://localhost:2002/api/identity"),
                    })
                 }

                 //new Parameters(new[]{
                 //    new KeyValuePair<string,string>("username","admin"),
                 //    new KeyValuePair<string, string>("password","123"),
                 //    new KeyValuePair<string, string>("returnUrl","https://localhost:2002/api/identity"),
                 //})),


            }); 

            _apiClient = new HttpClient(result.RefreshTokenHandler)
            {
                BaseAddress = new Uri("https://localhost:2002/api/identity")
            };

            ShowResult(result);
            await NextSteps(result);
        }

        private static void ShowResult(LoginResult result)
        {
            if (result.IsError)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.TokenResponse.Raw);

            Console.WriteLine($"token response...");
            foreach (var item in values)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "  x...exit  c...call api   ";
            if (currentRefreshToken != null) menu += "r...refresh token   ";

            while (true)
            {
                Console.WriteLine("\n\n");

                Console.Write(menu);
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.X) return;
                if (key.Key == ConsoleKey.C) await CallApi();
                if (key.Key == ConsoleKey.R)
                {
                    var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
                    if (refreshResult.IsError)
                    {
                        Console.WriteLine($"Error: {refreshResult.Error}");
                    }
                    else
                    {
                        currentRefreshToken = refreshResult.RefreshToken;
                        currentAccessToken = refreshResult.AccessToken;

                        Console.WriteLine("\n\n");
                        Console.WriteLine($"access token:   {currentAccessToken}");
                        Console.WriteLine($"refresh token:  {currentRefreshToken ?? "none"}");
                    }
                }
            }
        }

        private static async Task CallApi()
        {
            var response = await _apiClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Console.WriteLine("\n\n");
                Console.WriteLine(json.RootElement);
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }


    }
}
