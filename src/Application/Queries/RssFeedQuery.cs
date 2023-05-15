
using HySite.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HySite.Queries;

public class RssFeedQuery : IRequest<string>
{

}

public class RssFeedQueryHandler : IRequestHandler<RssFeedQuery, string>
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IConfiguration _configuration;
    private readonly IRssFeedService _rssFeedService;

    public RssFeedQueryHandler(IBlogPostRepository blogPostRepository, IConfiguration configuration, IRssFeedService rssFeedService)
    {
        _blogPostRepository = blogPostRepository;
        _configuration = configuration;
        _rssFeedService = rssFeedService;
    }

    public async Task<string> Handle(RssFeedQuery request, CancellationToken cancellationToken)
    {
        //TODO: create configuration repository?
        var path = _configuration["PostsLocalPath"];
        var fileName = _configuration["RssFeedFile"];
        var rssPath = String.Join('/', path, fileName);

        if(!System.IO.File.Exists(rssPath))
        {
            var posts = await _blogPostRepository
                .RetrieveAll()
                .ToListAsync(cancellationToken);

            _rssFeedService.CreateRssFeed(posts, rssPath);
        }

        return rssPath;
    }
}