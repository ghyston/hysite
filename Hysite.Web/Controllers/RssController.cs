using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hysite.Queries;
using MediatR;

namespace hySite;

public class RssController : Controller
{
    private readonly IMediator _mediator;

    public RssController(IMediator mediator) => this._mediator = mediator;

    [ResponseCache(Duration=60)]
    [HttpGet("/rss")]
    public async Task<FileStreamResult> Index()
    {
        var rssPath = await _mediator.Send(new RssFeedQuery());
        var stream = System.IO.File.OpenRead(rssPath);
        return File(stream, "application/rss+xml; charset=utf-8");
    }
}