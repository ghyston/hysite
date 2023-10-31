using System.Collections.Generic;
using System.Linq;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HySite.Web.Pages;

public class YearPageModel : PageModel
{
    private readonly IBlogPostRepository _blogPostRepository;
    
    public List<BlogPost> Posts { get; private set; } = new ();
    public int Year { get; set; }

    public YearPageModel(IBlogPostRepository blogPostRepository) => 
        _blogPostRepository = blogPostRepository;

    public IActionResult OnGet(int year)
    {
        Year = year;
        Posts = _blogPostRepository.FindPostsByYear(year).ToList();

        return Page();
    }
}