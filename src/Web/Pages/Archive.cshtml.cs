using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HySite.Web.Pages;

public class ArchivePageModel : PageModel
{
    private readonly IBlogPostRepository _blogPostRepository;

    public IEnumerable<int> Years { get; set; }
    public int PostsCount { get; set; }

    public ArchivePageModel(IBlogPostRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken) 
    {
        Years = await _blogPostRepository.GetAllYears(cancellationToken);
        PostsCount = _blogPostRepository.PostsCount();

        return Page();
    }
}