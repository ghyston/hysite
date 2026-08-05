using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HySite.Web.Pages;

public class TagPageModel : PageModel
{
    private readonly IBlogPostRepository _blogPostRepository;

    public List<BlogPost> Posts { get; private set; } = new ();
    public string Tag { get; set; } = string.Empty;

    public TagPageModel(IBlogPostRepository blogPostRepository) =>
        _blogPostRepository = blogPostRepository;

    public async Task<IActionResult> OnGetAsync(string tagName, CancellationToken cancellationToken)
    {
        Tag = tagName;
        Posts = await _blogPostRepository.FindPostsByTag(tagName, cancellationToken);

        return Page();
    }
}
