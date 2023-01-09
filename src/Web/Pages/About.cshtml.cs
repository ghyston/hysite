using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace hySite;

public class AboutModel : PageModel
{
    private readonly IVersionService _versionService;
    public string Version { get; set;}
    public string Framework { get; set; }

    public AboutModel(IVersionService versionService)
    {
        _versionService = versionService;
    }

    public IActionResult OnGet()
    {
        this.Version = _versionService.GetCurrentGitSHA();
        this.Framework = _versionService.GetFrameworkVersion();
        return Page();
    }
}