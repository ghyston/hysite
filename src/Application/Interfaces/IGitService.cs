using HySite.Application.Dto;

namespace HySite.Application.Interfaces;

public interface IGitService 
{
    void Clone(GitSettingsDto settings);
}