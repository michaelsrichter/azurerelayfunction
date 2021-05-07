using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Relay;
using System.Net;
using System.Text;

namespace listener
{
    class Program
    {
        private const string RelayNamespace = "<your-relay>.servicebus.windows.net";
        private const string ConnectionName = "<your-connection>";
        private const string KeyName = "<your key name - default is RootManageSharedAccessKey>";
        private const string Key = "<your key>";
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var listener = new HybridConnectionListener(new Uri(string.Format("sb://{0}/{1}", RelayNamespace, ConnectionName)), tokenProvider);

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                var name = context.Request.Headers["PassThrough"];
                // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "OK";

                var env = new StringBuilder();
                env.Append($"Machine Name = {Environment.MachineName}\r\n");
                env.Append($"OS Version = {Environment.OSVersion}\r\n");
                env.Append($"Current Directory = {Environment.CurrentDirectory}\r\n");
                env.Append($"Current User = {Environment.UserName}\r\n");

                using (var sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.WriteLine(env.ToString());
                    sw.WriteLine("hello, " + name);
                }
                Console.WriteLine("request from " + name);
                // The context MUST be closed here
                context.Response.Close();
            };

            // Opening the listener establishes the control channel to
            // the Azure Relay service. The control channel is continuously 
            // maintained, and is reestablished when connectivity is disrupted.
            await listener.OpenAsync();
            Console.WriteLine("Server listening");

            // Start a new thread that will continuously read the console.
            await Console.In.ReadLineAsync();

            // Close the listener after you exit the processing loop.
            await listener.CloseAsync();
        }
    }
}
