using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Markdig;

namespace hySite
{
    public class PostModel : PageModel
    {
        private readonly AppDbContext _db;

        [BindProperty]
        public BlogPost BlogPost { get; set; }

        public PostModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGet(string postName)
        {
            postName = postName.ToLower();
            BlogPost = _db.BlogPosts.Where(p => p.FileName == postName).FirstOrDefault();
            if(BlogPost == null)
            {
                return RedirectToPage("/LostAndNotFound");
            }

            return Page();
        }
    }
}
