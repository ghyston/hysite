public interface IGitRepository
{
    bool IsSecretValid(string token, string payload);
    void Clone();
    void Pull();
}