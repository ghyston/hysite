using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HySite.Application.Interfaces;
using HySite.Domain.Model;

namespace HySite.Web.Pages;

public class PostPageModel : PageModel
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IServiceProvider _serviceProvider;

    [BindProperty]
    public BlogPost BlogPost { get; set; }

    [BindProperty]
    public string NextPostFileName { get; set; }

    [BindProperty]
    public string PrevPostFileName { get; set; }

    public PostPageModel(IBlogPostRepository blogPostRepository, IServiceProvider serviceProvider)
    {
        _blogPostRepository = blogPostRepository;
        _serviceProvider = serviceProvider;
    }

    public IActionResult OnGet(string postName)
    {
        BlogPost = _blogPostRepository.FindPostByFileName(postName.ToLower());
        if (BlogPost == null)
        {
            return RedirectToPage("/LostAndNotFound");
        }

        //var handler = _serviceProvider.GetService<IncrementViewsHandler>();
        //(handler as IncrementViewsHandler)?
        //.Handle(new IncrementViewsHandlerRequest());

        NextPostFileName = _blogPostRepository.FindNextPostFileName(BlogPost.Created);
        PrevPostFileName = _blogPostRepository.FindPrevPostFileName(BlogPost.Created);

        return Page();
    }
}
