using System;
using System.Threading.Tasks;
using HySite.Application.Command;
using HySite.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HySite.Web.Controllers;

public class ThemeController : Controller
{
    private readonly IMediator _mediator;

    public ThemeController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Toggle()
    {
        var form = await Request.ReadFormAsync();
        var result = await _mediator.Send(new ToggleThemeCommand
        {
            CurrentTheme = ThemePreferenceHelper.ResolveTheme(Request.Cookies[ThemePreferenceHelper.CookieName]),
            RequestedTheme = ThemePreferenceHelper.ResolveTheme(form["theme"].ToString()),
            ReturnUrl = form["returnUrl"].ToString()
        });

        if (result.ShouldRemoveCookie)
        {
            Response.Cookies.Delete(ThemePreferenceHelper.CookieName);
        }
        else if (!string.IsNullOrEmpty(result.CookieValue))
        {
            Response.Cookies.Append(
                ThemePreferenceHelper.CookieName,
                result.CookieValue,
                CreateCookieOptions(result.CookieExpiresAt, Request.IsHttps));
        }

        return LocalRedirect(result.ReturnUrl);
    }

    private static CookieOptions CreateCookieOptions(DateTimeOffset expiresAt, bool isHttps) => new()
    {
        Expires = expiresAt,
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = isHttps
    };
}
