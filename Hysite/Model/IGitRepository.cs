public interface IGitRepository
{
    bool IsSecretValid(string token, string payload);
    IResult Clone();
    IResult Pull();
}