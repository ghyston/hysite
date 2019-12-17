using System;
using System.IO;

namespace hySite 
{
    public struct OnNewFileRequest
    {
        public string FilePath { get; set; }
    }

    public struct OnNewFileResponse
    {
    }

    public class OnNewFileHandler : IHandler<OnNewFileRequest, OnNewFileResponse>
    {
        private readonly IFileParserService _fileParserService;
        private readonly IBlogPostRepository _blogPostRepository;
        private AppDbContext _dbContext;

        public OnNewFileHandler(IFileParserService fileParserService, IBlogPostRepository blogPostRepository, AppDbContext dbContext)
        {
            _fileParserService = fileParserService;
            _blogPostRepository = blogPostRepository;
            _dbContext = dbContext;
        }

        public OnNewFileResponse Handle(OnNewFileRequest request)
        {
            var fileName = Path.GetFileNameWithoutExtension(request.FilePath).ToLower();
            var fileInfo = new FileInfo(request.FilePath);

            using(var reader = fileInfo.OpenText())
            {   
                var blogPost = _fileParserService.ParseFile(fileName, reader);
                _blogPostRepository.Add(blogPost);
                _dbContext.SaveChanges();
            }

            return new OnNewFileResponse();
        }
    }
}