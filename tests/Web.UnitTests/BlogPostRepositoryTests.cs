using FluentAssertions;
using HySite.Application.Repositories;
using HySite.Domain.Model;
using HySite.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Web.UnitTests;

public class BlogPostRepositoryTests
{
    [Fact]
    public async Task FindPostsByTagReturnsPostsWithTag()
    {
        // Given
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        var swiftTag = new BlogTag { Name = "swift" };
        var csharpTag = new BlogTag { Name = "csharp" };

        dbContext.BlogPosts.AddRange(
            new BlogPost
            {
                FileName = "swift-post",
                Title = "Swift Post",
                MdContent = string.Empty,
                HtmlContent = string.Empty,
                Created = new DateTime(2017, 1, 1),
                Tags = [swiftTag]
            },
            new BlogPost
            {
                FileName = "csharp-post",
                Title = "CSharp Post",
                MdContent = string.Empty,
                HtmlContent = string.Empty,
                Created = new DateTime(2017, 1, 2),
                Tags = [csharpTag]
            });
        await dbContext.SaveChangesAsync();

        // When
        var repository = new BlogPostRepository(dbContext);
        var result = await repository.FindPostsByTag("swift", CancellationToken.None);

        //Then
        result.Should().ContainSingle();
        result[0].FileName.Should().Be("swift-post");
    }
}
