using System.Collections.Generic;
using System.Linq;

namespace hySite
{
    class BlogPostRepository : IBlogPostRepository
    {
        private AppDbContext _dbContext;

        public BlogPostRepository(AppDbContext db)
        {
            _dbContext = db;
        }

        public void Add(BlogPost post)
        {
            _dbContext.Add(post);
        }

        public void Add(IEnumerable<BlogPost> posts)
        {
            _dbContext.AddRange(posts);
        }
            
        public BlogPost FindPostByFileName(string fileName)
        {
            return _dbContext
                .BlogPosts
                .Where(p => p.FileName == fileName)
                .FirstOrDefault();
        }

        public IEnumerable<BlogPost> FindPostsByPage(int pageNumber, int postPerPage)
        {
            return _dbContext
                .BlogPosts
                .OrderByDescending(p => p.Created)
                .Skip(pageNumber * postPerPage)
                .Take(postPerPage).ToList();

        }

        public int PostsCount()
        {
            return _dbContext.BlogPosts.Count();
        }

        public void RemoveAll()
        {
            _dbContext.BlogPosts.RemoveRange(_dbContext.BlogPosts);
        }
    }
}
