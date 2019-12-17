using System;
using System.IO;

namespace hySite
{
    public class OnFileDeletedRequest
    {
        public string FilePath { get; set; }
    }

    public class OnFileDeletedResponse
    {

    }

    public class OnFileDeletedHandler : IHandler<OnFileDeletedRequest, OnFileDeletedResponse>
    {
        private readonly IBlogPostRepository _blogPostRepository;
        private AppDbContext _dbContext;

        public OnFileDeletedHandler(IBlogPostRepository blogPostRepository, AppDbContext dbContext)
        {
            _blogPostRepository = blogPostRepository;
            _dbContext = dbContext;
        }

        public OnFileDeletedResponse Handle(OnFileDeletedRequest request)
        {
            //@todo: this is called several times.
            var fileName = Path.GetFileNameWithoutExtension(request.FilePath).ToLower();
            BlogPost oldPost = _blogPostRepository.FindPostByFileName(fileName);
            
            if(oldPost == null)
            {
                throw new Exception($"{typeof(OnFileDeletedHandler)}: Cannot delete post with file name '{fileName}', it is not exist.");
            }

            _blogPostRepository.Remove(oldPost);
            _dbContext.SaveChanges();

            return new OnFileDeletedResponse();
        }
    }
}