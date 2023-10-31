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

    public async Task<List<BlogPost>> FindPostsByYear(int year, CancellationToken cancellationToken) =>
        await PostsByYear(year)
            .OrderBy(bp => bp.Created)
            .ToListAsync(cancellationToken);

    public async Task<bool> AnyPostsAtYear(int year, CancellationToken cancellationToken) => 
        await PostsByYear(year).AnyAsync(cancellationToken);

    public IQueryable<BlogPost> RetrieveAll() =>
            _dbContext
            .BlogPosts
            .OrderByDescending(p => p.Created);

    public async Task<IEnumerable<int>> GetAllYears(CancellationToken cancellationToken) => 
        await _dbContext
            .BlogPosts
            .Where(bp => bp.Created != DateTime.MinValue)
            .Select(bp => bp.Created.Year)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

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
