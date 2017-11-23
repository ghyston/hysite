using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace hySite
{
    public class PostModel : PageModel
    {
        public List<string> Posts {get;} = new List<string>(){
            "*italic*", "**bold**" 
        };

        public List<string> MainContent {get; private set; }

        public string PostContent {get; private set; }

   /*     public void OnGet(int? pageNumber = 1)
        {
            var md = $@"
# Page N{pageNumber}  
Just some line
And another line
* this is one item  
* this is another";

            this.PostContent = Markdown.ToHtml(md);

            this.MainContent = Posts.Select(p => Markdown.ToHtml(p)).ToList();

            //@todo: load actual content from db

        }*/
    }
}
