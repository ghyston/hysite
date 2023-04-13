namespace HySite.Application.Dto;

public class GitSettingsDto
{
    public required string GitUrl { get; init; }
    public required string LocalPath { get; init; }
    public required string GitUser { get; init; }
    public required string GithubToken { get; init; }
}