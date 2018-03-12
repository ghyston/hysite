using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace hySite
{
    public class IndexModel : PageModel
    {
        public List<BlogPost> Posts {get; private set; } = new List<BlogPost>();
        public int PageNum {get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; } //@todo: make it string? to hide "/0" at first page
        
        private const int POSTS_PER_PAGE = 5;

        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IConfiguration _configuration;

        public IndexModel(IBlogPostRepository blogPostRepository, IConfiguration configuration)
        {
            _blogPostRepository = blogPostRepository;
            _configuration = configuration;
        }

        public void OnGet(int pageNumber)
        {
            //const int POSTS_PER_PAGE = (Int32.Parse() ?? 5) as int;
            var pagesCount = _blogPostRepository.PostsCount() / POSTS_PER_PAGE;
            this.PageNum = pageNumber;            
            this.PrevPage = pageNumber == 0 ? (int?)null : (pageNumber - 1);            
            this.NextPage = pageNumber >= pagesCount ? (int?)null : (pageNumber + 1);
            this.Posts = _blogPostRepository.FindPostsByPage(this.PageNum, POSTS_PER_PAGE).ToList();
        }
    }
}
