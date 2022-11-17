using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using hySite;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace hysite.Commands;

public class UpdatePostsCommand : IRequest
{
    public string Signature { get; set; }
    public Stream Payload { get; set; }
}

public class UpdatePostsCommandHandler : IRequestHandler<UpdatePostsCommand>
{
    private readonly IGitRepository _gitRepository;
    private readonly IConfiguration _configuration;
    private readonly IRssFeedService _rssFeedService;
    private readonly IFileParserService _fileParserService;

    public UpdatePostsCommandHandler(IGitRepository gitRepository, IConfiguration configuration, IRssFeedService rssFeedService, IFileParserService fileParserService)
    {
        _gitRepository = gitRepository;
        _configuration = configuration;
        _rssFeedService = rssFeedService;
        _fileParserService = fileParserService;
    }

    public async Task<Unit> Handle(UpdatePostsCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Payload);

        var payload = await reader.ReadToEndAsync();
        if (!_gitRepository.IsSecretValid(request.Signature, payload))
            throw new SecurityException(); //TODO: implement NotAuthorisedException, catch it in filter and return Http Unauthorized()

        var path = _configuration["PostsLocalPath"];
        var fileName = _configuration["RssFeedFile"];
        var rssPath = String.Join('/', path, fileName);

        _gitRepository.Pull();
        _fileParserService.ParseExistingFiles();
        await _rssFeedService.CreateRssFeed(rssPath, cancellationToken);

        return Unit.Value;
    }
}