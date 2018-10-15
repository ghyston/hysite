using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


public class PingController : Controller 
{
    [Route("ping")]
    public IActionResult Index()
    {
        return Ok();
    }
    
}