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
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace hySite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public interface ISecretsProvider {

            string GetSecret(string name);
        }

        //TODO: inject logger, move to separate class etc
        public class AzureKeyVaultSecretsProvider : ISecretsProvider {
            private static string keyVaultName = "hyston-blog-kv";

            //TODO: not url should be lazy, but client itself. And it may be static (or use proper DI strategy)
            private Lazy<Uri> KeyVaultUrl => new Lazy<Uri>(() => new Uri($"https://{keyVaultName}.vault.azure.net/"));

            public string GetSecret(string secretName) 
            {
                try {
                    var client = new SecretClient(KeyVaultUrl.Value, new DefaultAzureCredential());
                    var userSecret = client.GetSecret(secretName);
                    return userSecret.Value.Value;
                }
                catch (Exception e) {
                    //TODO: add logger
                    return string.Empty;
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) => {

                    try
                    {
                        ISecretsProvider vault = new AzureKeyVaultSecretsProvider();
                        var hook = vault.GetSecret("github-user");

                        var keyVaultName = "hyston-blog-kv";
                        var kvUri = "https://" + keyVaultName + ".vault.azure.net/";

                        var tokenCredential = new ClientSecretCredential(
                            tenantId: "???",
                            clientId: "???",
                            clientSecret: "???"
                            ); //TODO: use DefaultAzureCredentical when all params would be in environment

                        //tokenCredential = new DefaultAzureCredential();

                        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

                        var userSecret = client.GetSecret("github-user");
                        var passSecret = client.GetSecret("github-pass");
                        var hookSecret = client.GetSecret("github-hook");
                        
                    }
                    catch(Exception) 
                    {

                    }
                })
                .UseStartup<Startup>()
                .Build();

        private static string GetKeyVaultEndpoint() => "https://hyston-blog-kv.vault.azure.net/";
    }
}
