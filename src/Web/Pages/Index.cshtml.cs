using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace HySite.Web.Pages;

public class IndexModel : PageModel
{
    public List<BlogPost> Posts { get; private set; } = new ();
    public int PageNum { get; set; }
    public int? NextPage { get; set; }
    public int? PrevPage { get; set; } //@todo: make it string? to hide "/0" at first page
    public string Version { get; set; }

    private static int POSTS_PER_PAGE = 5;

    private readonly IConfiguration _configuration;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IVersionService _versionService;

    public IndexModel(IConfiguration configuration, IBlogPostRepository blogPostRepository, IVersionService versionService)
    {
        _configuration = configuration;
        _blogPostRepository = blogPostRepository;
        _versionService = versionService;
        
        POSTS_PER_PAGE = int.Parse(_configuration["PostsPerPage"]);
    }

    public async Task<IActionResult> OnGet(int pageNumber, CancellationToken cancellationToken)
    {
        var posts = await _blogPostRepository.FindPostsByYear(year: pageNumber, cancellationToken);
        if(posts.Any())
            return RedirectToPage("./Year", new { Year = pageNumber});

        int postsCount = _blogPostRepository.PostsCount();
        var pagesCount = postsCount / POSTS_PER_PAGE
            - (postsCount % POSTS_PER_PAGE == 0 ? 1 : 0);

        if (pageNumber > pagesCount)
        {
            return RedirectToPage("/LostAndNotFound");
        }

        PageNum = pageNumber;
        PrevPage = pageNumber >= pagesCount ? null : (pageNumber + 1);
        NextPage = pageNumber == 0 ? null : (pageNumber - 1);
        Posts = _blogPostRepository
            .FindPostsByPage(this.PageNum, POSTS_PER_PAGE)
            .ToList();

        //TODO: implement incrementation
        //var handler = _serviceProvider.GetService<IncrementViewsHandler>();
        //(handler as IncrementViewsHandler)?
        //.Handle(new IncrementViewsHandlerRequest());

        Version = _versionService.GetCurrentGitSHA();

        return Page();
    }
}
