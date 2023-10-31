using System.Collections.Generic;
using System.Linq;
using System;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace HySite.Application.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private IHysiteContext _dbContext;

    public BlogPostRepository(IHysiteContext db)
    {
        _dbContext = db;
    }

    public void Add(BlogPost post) => _dbContext.BlogPosts.Add(post);

    public void Add(IEnumerable<BlogPost> posts) => _dbContext.BlogPosts.AddRange(posts);

    public BlogPost? FindPostByFileName(string fileName) => 
        _dbContext
            .BlogPosts
            .Where(p => p.FileName == fileName)
            .FirstOrDefault();

    public IEnumerable<BlogPost> FindPostsByPage(int pageNumber, int postPerPage) => 
        _dbContext
            .BlogPosts
            .OrderByDescending(p => p.Created)
            .Skip(pageNumber * postPerPage)
            .Take(postPerPage).ToList();

    private IQueryable<BlogPost> PostsByYear(int year) => 
        _dbContext
            .BlogPosts
            .Where(bp => 
                bp.Created != DateTime.MinValue && 
                bp.Created.Year == year);

    public IEnumerable<BlogPost> FindPostsByYear(int year) =>
        PostsByYear(year)
            .OrderBy(bp => bp.Created)
            .ToList();

    public bool AnyPostsAtYear(int year) => 
        PostsByYear(year).Any();

    public IQueryable<BlogPost> RetrieveAll() =>
            _dbContext
            .BlogPosts
            .OrderByDescending(p => p.Created);

    public IEnumerable<int> GetAllYears() => 
        _dbContext
            .BlogPosts
            .Where(bp => bp.Created != DateTime.MinValue)
            .Select(bp => bp.Created.Year)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

    public int PostsCount() => _dbContext.BlogPosts.Count();

    public void Remove(BlogPost post) => _dbContext.BlogPosts.Remove(post);

    public void RemoveAll() => _dbContext.BlogPosts.RemoveRange(_dbContext.BlogPosts);

    public string? FindNextPostFileName(DateTime time) => 
        _dbContext.BlogPosts
            .Where(bp => bp.Created > time)
            .OrderBy(bp => bp.Created)
            .Select(bp => bp.FileName)
            .FirstOrDefault();

    public string? FindPrevPostFileName(DateTime time) => 
        _dbContext.BlogPosts
            .Where(bp => bp.Created < time)
            .OrderByDescending(bp => bp.Created)
            .Select(bp => bp.FileName)
            .FirstOrDefault();
}
