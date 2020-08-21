namespace hySite
{
    public interface ISecretsProvider 
    {
        string GetSecret(string name);
    }
}