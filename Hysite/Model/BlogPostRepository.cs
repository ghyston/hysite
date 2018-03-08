using System.Collections.Generic;
using System.Linq;
using System;

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

        public void Remove(BlogPost post)
        {
            _dbContext.BlogPosts.Remove(post);
        }

        public void RemoveAll()
        {
            _dbContext.BlogPosts.RemoveRange(_dbContext.BlogPosts);
        }

        public string FindNextPostFileName(DateTime time)
        {
            return _dbContext.BlogPosts
                .Where(bp => bp.Created > time)
                .OrderBy(bp => bp.Created)
                .Select(bp => bp.FileName)
                .FirstOrDefault();
        }

        public string FindPrevPostFileName(DateTime time)
        {
            return _dbContext.BlogPosts
                .Where(bp => bp.Created < time)
                .OrderByDescending(bp => bp.Created)
                .Select(bp => bp.FileName)
                .FirstOrDefault();

        }
    }
}
