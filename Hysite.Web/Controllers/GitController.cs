using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hysite.Commands;
using MediatR;

namespace hySite; 

public class GitController : Controller 
{
    private readonly IMediator _mediator;

    public GitController(IMediator mediator) => _mediator = mediator;

    [Route("update")]
    public async Task<IActionResult> UpdatePostsAsync()
    {
        var command = new UpdatePostsCommand 
        {
            Signature = Request.Headers["X-Hub-Signature"],
            Payload = Request.Body
        };

        await _mediator.Send(command);
        return Ok();
    }
}

