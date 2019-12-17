using System;
using System.IO;

namespace hySite
{
    public class OnFileRenamedRequest
    {
        public string OldFilePath { get; set; }
        public string NewFilePath { get; set; }
    }

    public class OnFileRenamedResponse
    {

    }

    public class OnFileRenamedHandler : IHandler<OnFileRenamedRequest, OnFileRenamedResponse>
    {
        private readonly IBlogPostRepository _blogPostRepository;
        private AppDbContext _dbContext;

        public OnFileRenamedHandler(IBlogPostRepository blogPostRepository, AppDbContext dbContext)
        {
            _blogPostRepository = blogPostRepository;
            _dbContext = dbContext;
        }

        public OnFileRenamedResponse Handle(OnFileRenamedRequest request)
        {
            //@todo: this is called several times.
            var oldFileName = Path.GetFileNameWithoutExtension(request.OldFilePath).ToLower();
            var newFileName = Path.GetFileNameWithoutExtension(request.NewFilePath).ToLower();
            BlogPost post = _blogPostRepository.FindPostByFileName(oldFileName);
            
            if(post == null)
            {
                throw new Exception($"{typeof(OnFileRenamedHandler)}: Cannot rename post with file name '{oldFileName}' to new file name '{newFileName}', it is not exist.");
            }

            post.FileName = newFileName;
            _dbContext.SaveChanges();
                
            return new OnFileRenamedResponse();
        }
    }
}
