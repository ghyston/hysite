using System;
using System.Collections.Generic;
using System.Linq;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace HySite.Web.Pages;

public class IndexModel : PageModel
{
    public List<BlogPost> Posts { get; private set; } = new List<BlogPost>();
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
        
        POSTS_PER_PAGE = Int32.Parse(_configuration["PostsPerPage"]);
    }

    public IActionResult OnGet(int pageNumber)
    {
        int postsCount = _blogPostRepository.PostsCount();
        var pagesCount = postsCount / POSTS_PER_PAGE
            - (postsCount % POSTS_PER_PAGE == 0 ? 1 : 0);

        if (pageNumber > pagesCount)
        {
            return RedirectToPage("/LostAndNotFound");
        }

        this.PageNum = pageNumber;
        this.PrevPage = pageNumber >= pagesCount ? (int?)null : (pageNumber + 1);
        this.NextPage = pageNumber == 0 ? (int?)null : (pageNumber - 1);
        this.Posts = _blogPostRepository
            .FindPostsByPage(this.PageNum, POSTS_PER_PAGE)
            .ToList();

        //TODO: implement incrementation
        //var handler = _serviceProvider.GetService<IncrementViewsHandler>();
        //(handler as IncrementViewsHandler)?
        //.Handle(new IncrementViewsHandlerRequest());

        this.Version = _versionService.GetCurrentGitSHA();

        return Page();
    }
}
