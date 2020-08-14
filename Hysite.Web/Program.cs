using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace hySite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) => {

                    //this code was taken from https://www.youtube.com/watch?v=k2VYcYS3EIA&t=846s
                    var keyVaultEndpoint = GetKeyVaultEndpoint();
                    if( string.IsNullOrWhiteSpace(keyVaultEndpoint))
                        return;

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var authCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                    var keyVaultClient = new KeyVaultClient(authCallback);
                    builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                })
                .UseStartup<Startup>()
                .Build();

        private static string GetKeyVaultEndpoint() => "https://kv-hysite.vault.azure.net/";
    }
}
