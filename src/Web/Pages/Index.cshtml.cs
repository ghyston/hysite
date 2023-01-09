using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace hySite
{
    public class IndexModel : PageModel
    {
        public List<BlogPost> Posts {get; private set; } = new List<BlogPost>();
        public int PageNum {get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; } //@todo: make it string? to hide "/0" at first page
        public string Version { get; set;}
        
        private static int POSTS_PER_PAGE = 5;

        private readonly IConfiguration _configuration;


        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
            POSTS_PER_PAGE = Int32.Parse(_configuration["PostsPerPage"]);
        }

        public IActionResult OnGet(int pageNumber) => RedirectToPage("/UnderConstruction");
    }
}
