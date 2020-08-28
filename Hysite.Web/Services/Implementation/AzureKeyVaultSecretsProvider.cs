using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace hySite
{
    public class AzureKeyVaultSecretsProvider : ISecretsProvider 
    {
        private readonly ILogger<AzureKeyVaultSecretsProvider> logger;

        private static string keyVaultName = "hyston-blog-kv";
        private static Uri KeyVaultUrl(string keyVaultName) => new Uri($"https://{keyVaultName}.vault.azure.net/");
        private Lazy<SecretClient> LazySecretClient = new Lazy<SecretClient>(() => new SecretClient(KeyVaultUrl(keyVaultName), new DefaultAzureCredential()));
        private SecretClient Client => LazySecretClient.Value;

        public AzureKeyVaultSecretsProvider(ILogger<AzureKeyVaultSecretsProvider> logger)
        {
            this.logger = logger;
        }

        public string GetSecret(string secretName) 
        {
            try {
                var userSecret = Client.GetSecret(secretName);
                return userSecret.Value.Value;
            }
            catch (Exception e) {
                logger.LogError(e, $"Failed to retrieve secret '{secretName}'");
                return string.Empty;
            }
        }
    }
}