using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Azure.Relay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace client
{
    public static class client
    {
        private const string RelayNamespace = "<your-relay>.servicebus.windows.net";
        private const string ConnectionName = "<your-connection>";
        private const string KeyName = "<your key name - default is RootManageSharedAccessKey>";
        private const string Key = "<your key>";


        [FunctionName("client")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var response = await RunAsync(name);

            return new OkObjectResult(response);
        }


        private static async Task<string> RunAsync(string name)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
             KeyName, Key);
            var uri = new Uri(string.Format("https://{0}/{1}", RelayNamespace, ConnectionName));
            var token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            request.Headers.Add("PassThrough", new[] { name });
            request.Headers.Add("ServiceBusAuthorization", token);
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
