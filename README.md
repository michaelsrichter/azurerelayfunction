# Azure Relay Function
### Demo of using Azure Relay with Functions

To get this demo to work, follow these easy steps. 

* Create a [Relay in Azure](https://docs.microsoft.com/en-us/azure/azure-relay/relay-create-namespace-portal).
* Create a Connection in the Relay
* Add the Relay and Connection details to the `Program.cs` and `client.cs` files.
* Deploy the **listener** project to a server somewhere (like your laptop or a Raspberry Pi!). Run `dotnet build` and `dotnet run`.
* Deploy the **client** project to Azure functions. 
* Hit the client project Azure function URL like this: 
  * `https://<your function>.azurewebsites.net/api/client?name=put your name here`
* You should get a response from your listener via the Azure Relay.
