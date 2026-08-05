using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Interfaces;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HySite.Web.Pages;

public class ArchivePageModel(IBlogPostRepository blogPostRepository) : PageModel
{
    private readonly IBlogPostRepository _blogPostRepository = blogPostRepository;

    public IEnumerable<int> Years { get; set; }
    public IEnumerable<string> Tags { get; set; }
    public int PostsCount { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken) 
    {
        Years = await _blogPostRepository.GetAllYears(cancellationToken);
        Tags = await _blogPostRepository.GetAllTags(cancellationToken);
        PostsCount = _blogPostRepository.PostsCount();

        return Page();
    }
}