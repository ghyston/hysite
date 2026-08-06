using FluentAssertions;
using HySite.Application.Command;
using HySite.Application.Helpers;
using HySite.Domain.Enums;
using HySite.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Web.UnitTests;

public class ThemeControllerTests
{
    [Fact]
    public async Task ToggleSendsThemeCommandAndWritesResultCookie()
    {
        ToggleThemeCommand? command = null;
        var expiresAt = DateTimeOffset.UtcNow.AddDays(1);
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ToggleThemeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ToggleThemeResult>, CancellationToken>((request, _) => command = (ToggleThemeCommand)request)
            .ReturnsAsync(new ToggleThemeResult
            {
                Theme = Theme.Dark,
                CookieValue = "dark",
                ShouldRemoveCookie = false,
                ReturnUrl = "/Archive",
                CookieExpiresAt = expiresAt
            });

        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{ThemePreferenceHelper.CookieName}=light";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["returnUrl"] = "/Archive",
            ["theme"] = Theme.Dark.ToString()
        });

        var controller = new ThemeController(mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context
            }
        };

        var result = await controller.Toggle();

        result.Should().BeOfType<LocalRedirectResult>()
            .Which.Url.Should().Be("/Archive");
        command.Should().NotBeNull();
        command!.CurrentTheme.Should().Be(Theme.Light);
        command.RequestedTheme.Should().Be(Theme.Dark);
        command.ReturnUrl.Should().Be("/Archive");
        context.Response.Headers.SetCookie.ToString().Should().Contain($"{ThemePreferenceHelper.CookieName}=dark");
    }

    [Fact]
    public async Task ToggleDeletesThemeCookieWhenCommandReturnsAutoTheme()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ToggleThemeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToggleThemeResult
            {
                Theme = Theme.Auto,
                CookieValue = null,
                ShouldRemoveCookie = true,
                ReturnUrl = "/",
                CookieExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
            });

        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{ThemePreferenceHelper.CookieName}=dark";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["returnUrl"] = "/",
            ["theme"] = Theme.Auto.ToString()
        });

        var controller = new ThemeController(mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context
            }
        };

        await controller.Toggle();

        var setCookie = context.Response.Headers.SetCookie.ToString();
        setCookie.Should().Contain($"{ThemePreferenceHelper.CookieName}=");
        setCookie.Should().Contain("expires=Thu, 01 Jan 1970");
    }
}
