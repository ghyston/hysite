using Microsoft.AspNetCore.Mvc;

namespace hySite; 

public class PingController : Controller 
{
    [Route("ping")]
    public IActionResult Index() => Ok();

}

