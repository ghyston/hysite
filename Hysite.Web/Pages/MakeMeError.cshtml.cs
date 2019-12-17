using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Markdig;

namespace hySite
{
    public class MakeMeErrorPage : PageModel
    {
        

        public IActionResult OnGet(int someNumber)
        {
            throw new Exception ("This is test exception messasge");
        }

    }

}
