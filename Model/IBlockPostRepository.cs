using System.Collections.Generic;

namespace hySite
{
    public interface IBlogPostRepository
    {
        void Add(BlogPost post);
        
        BlogPost FindPostByFileName(string fileName);

        IEnumerable<BlogPost> FindPostsByPage(int pageNumber, int postPerPage);

        int PostsCount();
    }

}
