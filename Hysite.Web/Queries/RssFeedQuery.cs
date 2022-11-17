using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using hySite;
using MediatR;

namespace hysite.Queries;

public class RssFeedQuery : IRequest<string>
{

}

public class RssFeedQueryHandler : IRequestHandler<RssFeedQuery, string>
{   
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IRssFeedService _rssFeedService;

    public RssFeedQueryHandler(AppDbContext dbContext, IConfiguration configuration, IRssFeedService rssFeedService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _rssFeedService = rssFeedService;
    }

    public async Task<string> Handle(RssFeedQuery request, CancellationToken cancellationToken)
    {
        var path = _configuration["PostsLocalPath"];
        var fileName = _configuration["RssFeedFile"];
        var rssPath = String.Join('/', path, fileName);

        if(!System.IO.File.Exists(rssPath))
            await _rssFeedService.CreateRssFeed(rssPath, cancellationToken);

        return rssPath;
    }
}