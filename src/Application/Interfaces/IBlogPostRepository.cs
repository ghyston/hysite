using System;
using System.Collections.Generic;
using HySite.Domain.Model;

namespace HySite.Application.Interfaces;

public interface IBlogPostRepository
{
    void Add(BlogPost post);
    void Add(IEnumerable<BlogPost> posts);
    BlogPost? FindPostByFileName(string fileName);
    IEnumerable<BlogPost> FindPostsByPage(int pageNumber, int postPerPage);
    bool AnyPostsAtYear(int year);
    IEnumerable<BlogPost> FindPostsByYear(int year);
    IQueryable<BlogPost> RetrieveAll();
    int PostsCount();
    void Remove(BlogPost post);
    void RemoveAll();
    string? FindNextPostFileName(DateTime time);
    string? FindPrevPostFileName(DateTime time);
}
