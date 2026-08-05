using FluentAssertions;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using HySite.Web.Pages;
using Moq;

namespace Web.UnitTests;

public class TagPageModelTests
{
    [Fact]
    public async Task OnGetRequestsPostsByTag()
    {
        // Given
        const string tagName = "swift";
        var cancellationToken = CancellationToken.None;
        List<BlogPost> posts =
        [
            new()
            {
                FileName = "swift-post",
                Title = "Swift Post",
                MdContent = string.Empty,
                HtmlContent = string.Empty,
                Created = new DateTime(2017, 1, 1)
            }
        ];

        var blogPostRepositoryMock = new Mock<IBlogPostRepository>();
        blogPostRepositoryMock
            .Setup(m => m.FindPostsByTag(tagName, cancellationToken))
            .ReturnsAsync(posts);

        // When
        var page = new TagPageModel(blogPostRepositoryMock.Object);
        await page.OnGetAsync(tagName, cancellationToken);

        //Then
        page.Tag.Should().Be(tagName);
        page.Posts.Should().BeEquivalentTo(posts);
    }
}
