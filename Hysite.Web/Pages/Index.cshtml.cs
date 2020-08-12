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
    //@todo: update handler register system!
    using IncrementViewsHandler = IHandler<IncrementViewsHandlerRequest, IncrementViewsHandlerResponse>;


    public class IndexModel : PageModel
    {
        public List<BlogPost> Posts {get; private set; } = new List<BlogPost>();
        public int PageNum {get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; } //@todo: make it string? to hide "/0" at first page
        public string Version { get; set;}
        
        private static int POSTS_PER_PAGE = 5;

        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public IndexModel(IBlogPostRepository blogPostRepository, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _blogPostRepository = blogPostRepository;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            POSTS_PER_PAGE = Int32.Parse(_configuration["PostsPerPage"]);
        }

        public IActionResult OnGet(int pageNumber)
        {   
            int postsCount = _blogPostRepository.PostsCount();
            var pagesCount = postsCount / POSTS_PER_PAGE 
                - (postsCount % POSTS_PER_PAGE == 0 ? 1 : 0);

            if(pageNumber > pagesCount)
            {
                return RedirectToPage("/LostAndNotFound");
            }

            this.PageNum = pageNumber;
            this.PrevPage = pageNumber >= pagesCount ? (int?)null : (pageNumber + 1);
            this.NextPage = pageNumber == 0 ? (int?)null : (pageNumber - 1);
            this.Posts = _blogPostRepository.FindPostsByPage(this.PageNum, POSTS_PER_PAGE).ToList();

            var handler = _serviceProvider.GetService<IncrementViewsHandler>();
                (handler as IncrementViewsHandler)?
                .Handle(new IncrementViewsHandlerRequest());

            this.Version = Environment.GetEnvironmentVariable("HYSITE_VERSION") ?? "unknown";

            return Page();
        }
    }
}
