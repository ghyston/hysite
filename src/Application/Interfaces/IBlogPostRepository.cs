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
    Task<bool> AnyPostsAtYear(int year, CancellationToken cancellationToken);
    Task<List<BlogPost>> FindPostsByYear(int year, CancellationToken cancellationToken);
    Task<IEnumerable<int>> GetAllYears(CancellationToken cancellationToken);
    IQueryable<BlogPost> RetrieveAll();
    int PostsCount();
    void Remove(BlogPost post);
    void RemoveAll();
    string? FindNextPostFileName(DateTime time);
    string? FindPrevPostFileName(DateTime time);
}
