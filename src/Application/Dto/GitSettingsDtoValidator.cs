using FluentValidation;

namespace HySite.Application.Dto;

public class GitSettingsDtoValidator : AbstractValidator<GitSettingsDto>
{
    public GitSettingsDtoValidator()
    {
        RuleFor(dto => dto.GitUrl).NotEmpty();
        RuleFor(dto => dto.LocalPath).NotEmpty();
        RuleFor(dto => dto.GitUser).NotEmpty();
        RuleFor(dto => dto.GithubToken).NotEmpty();
    }
}