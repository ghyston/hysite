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
        public List<string> Posts {get; private set; } = new List<string>();

        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
            this.Posts = _db.BlogPosts.Select(p => p.HtmlContent).ToList();
            Console.WriteLine("Index.onGet");
        }
    }
}
