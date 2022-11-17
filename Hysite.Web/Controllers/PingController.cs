using Microsoft.AspNetCore.Mvc;

using hysite.Queries;

using MediatR;
using System.Threading.Tasks;

namespace hySite; 

public class PingController : Controller 
{
    private readonly IMediator _mediatr;

    public PingController(IMediator mediatr) => this._mediatr = mediatr;

    [Route("ping")]
    public IActionResult Index() => Ok();

    [Route("lastweek")]
    public async Task<IActionResult> LastWeekViews() => 
        Ok(await _mediatr.Send(new LastWeekViewsQuery()));
}

