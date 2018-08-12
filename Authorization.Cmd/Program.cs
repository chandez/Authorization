using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization.Cmd
{
    class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        private static async Task MainAsync()
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token - client credentials client
            var tokenClient = new TokenClient(disco.TokenEndpoint, "client1Config", "passwordSecret1");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            // request token - resource owner password grant client
            //var tokenClient = new TokenClient(disco.TokenEndpoint, "client2Config", "passwordSecret2");
            //var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("chan@mail.com", "123456", "api1");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
