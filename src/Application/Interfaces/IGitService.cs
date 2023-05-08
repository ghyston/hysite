using HySite.Application.Dto;

namespace HySite.Application.Interfaces;

public interface IGitService 
{
    void Clone(GitSettingsDto settings);
    void Pull(GitSettingsDto settings);
    bool IsSecretValid(string signatureWithPrefix, string payload);
}