using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace hySite
{
    public class IndexModel : PageModel
    {
        public List<BlogPost> Posts {get; private set; } = new List<BlogPost>();
        public int PageNum {get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; } //@todo: make it string? to hide "/0" at first page
        
        private const int POSTS_PER_PAGE = 5;

        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public void OnGet(int pageNumber)
        {
            this.PageNum = pageNumber;            
            this.PrevPage = pageNumber == 0 ? (int?)null : (pageNumber - 1);
            var pages = _db.BlogPosts.Count() / POSTS_PER_PAGE;
            this.NextPage = pageNumber >= pages ? (int?)null : (pageNumber + 1);
            this.Posts = _db.BlogPosts.OrderByDescending(p => p.Created).Skip(this.PageNum * POSTS_PER_PAGE).Take(POSTS_PER_PAGE).ToList();
            //@todo: move to repository
        }
    }
}
