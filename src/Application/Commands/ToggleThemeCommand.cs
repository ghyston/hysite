using HySite.Application.Helpers;
using HySite.Domain.Enums;
using MediatR;

namespace HySite.Application.Command;

public class ToggleThemeCommand : IRequest<ToggleThemeResult>
{
    public Theme CurrentTheme { get; set; }
    public Theme RequestedTheme { get; set; }
    public string? ReturnUrl { get; set; }
}

public class ToggleThemeResult
{
    public required Theme Theme { get; set; }
    public required string? CookieValue { get; set; }
    public required bool ShouldRemoveCookie { get; set; }
    public required string ReturnUrl { get; set; }
    public required DateTimeOffset CookieExpiresAt { get; set; }
}

public class ToggleThemeCommandHandler : IRequestHandler<ToggleThemeCommand, ToggleThemeResult>
{
    public Task<ToggleThemeResult> Handle(ToggleThemeCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(new ToggleThemeResult
        {
            Theme = request.RequestedTheme,
            CookieValue = ThemePreferenceHelper.GetCookieValue(request.RequestedTheme),
            ShouldRemoveCookie = request.RequestedTheme == Theme.Auto,
            ReturnUrl = ThemePreferenceHelper.NormalizeReturnUrl(request.ReturnUrl),
            CookieExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        });
}
