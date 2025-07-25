using System.Collections.Generic;
using System.Linq;
using System;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Microsoft.EntityFrameworkCore;
using HySite.Domain.Dtos;

namespace HySite.Application.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly IHysiteContext _dbContext;

    public BlogPostRepository(IHysiteContext db)
    {
        _dbContext = db;
    }

    public void Add(BlogPost post) => _dbContext.BlogPosts.Add(post);

    public void Add(IEnumerable<BlogPost> posts) => _dbContext.BlogPosts.AddRange(posts);

    //TODO: tests!
    public async Task<IEnumerable<BlogPost>> CreatePosts(IEnumerable<BlogPostDto> dtos, CancellationToken cancellationToken)
    {
        var allTags = await _dbContext.BlogTags.ToListAsync(cancellationToken);

        BlogTag GetOrCreateTag(string tag)
        {
            var trimTag = tag.Trim();
            var blogTag = allTags.FirstOrDefault(t => t.Name == trimTag);
            if (blogTag != null)
                return blogTag;

            blogTag = new BlogTag
            {
                Name = trimTag
            };
            allTags.Add(blogTag);
            _dbContext.BlogTags.Add(blogTag);
            return blogTag;
        }

        return dtos
            .Select(p => new BlogPost
            {
                FileName = p.FileName,
                Title = p.Title,
                MdContent = p.Content,
                HtmlContent = string.Empty,
                Created = p.Created,
                Tags = p.Tags.Select(GetOrCreateTag).ToList()
            })
            .ToList();
    }

    public BlogPost? FindPostByFileName(string fileName) =>
        _dbContext
            .BlogPosts
            .Include(bp => bp.Tags)
            .Where(p => p.FileName == fileName)
            .FirstOrDefault();

    public IEnumerable<BlogPost> FindPostsByPage(int pageNumber, int postPerPage) =>
        _dbContext
            .BlogPosts
            .Include(bp => bp.Tags)
            .OrderByDescending(p => p.Created)
            .Skip(pageNumber * postPerPage)
            .Take(postPerPage).ToList();

    private IQueryable<BlogPost> PostsByYear(int year) =>
        _dbContext
            .BlogPosts
            .Include(bp => bp.Tags)
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
            .Include(bp => bp.Tags)
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
