using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hySite 
{
    public class GitController : Controller 
    {
        [Route("update")]
        public IActionResult UpdatePosts()
        {
            
            //@todo
            return Ok();
        }
    }
}

