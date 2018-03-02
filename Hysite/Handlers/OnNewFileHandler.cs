using System;
using System.IO;

namespace hySite 
{
    //@todo: make it common, etc
    public interface IOnNewHandler
    {
        void Handle(string filePath);
    }

    public class OnNewFileHandler : IOnNewHandler
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

        public void Handle(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);

            using(var reader = fileInfo.OpenText())
            {
                try {
                    var blogPost = _fileParserService.ParseFile(fileName, reader);
                    _blogPostRepository.Add(blogPost);
                    _dbContext.SaveChanges();
                }
                catch(Exception e)
                {
                    //@todo: log it
                }
            }
        }


    }

}