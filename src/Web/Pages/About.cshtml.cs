using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HySite.Application.Interfaces;
using HySite.Domain.Model;

namespace HySite.Web.Pages;

public class AboutModel : PageModel
{
    private readonly IVersionService _versionService;
    public string Version { get; set;}
    public string Framework { get; set; }

    public AboutModel(IVersionService versionService)
    {
        _versionService = versionService;
        Version = _versionService.GetCurrentGitSHA();
        Framework = _versionService.GetFrameworkVersion();
    }

    public IActionResult OnGet => Page();
}