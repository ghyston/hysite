using FluentAssertions;
using HySite.Application.Interfaces;
using HySite.Web.Pages;
using Moq;

namespace Web.UnitTests;

public class AboutModelTests
{
    [Fact]
    public void OnGetRequestsProperValues()
    {
        // Given
        var versionMock = new Mock<IVersionService>();
        versionMock.Setup(m => m.GetCurrentGitSHA()).Returns("123456");
        versionMock.Setup(m => m.GetFrameworkVersion()).Returns("qbasic");

        // When
        var page = new AboutModel(versionMock.Object);

        //Then
        page.Framework.Should().Be("qbasic");
        page.Version.Should().Be("123456");
    }
}