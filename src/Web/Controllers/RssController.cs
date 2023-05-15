using System.Threading.Tasks;
using HySite.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace hysite.Controllers;

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