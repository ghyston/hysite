using Microsoft.AspNetCore.Mvc;

namespace HySite.Web.Controllers;

public class PingController : Controller 
{
    [Route("ping")]
    public IActionResult Index() => Ok();

}

