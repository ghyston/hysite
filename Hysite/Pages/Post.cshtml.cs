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
        private readonly IBlogPostRepository _blogPostRepository;

        [BindProperty]
        public BlogPost BlogPost { get; set; }

        public PostModel(IBlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
        }

        public IActionResult OnGet(string postName)
        {
            BlogPost = _blogPostRepository.FindPostByFileName(postName.ToLower());
            if(BlogPost == null)
            {
                return RedirectToPage("/LostAndNotFound");
            }

            return Page();
        }
    }
}
