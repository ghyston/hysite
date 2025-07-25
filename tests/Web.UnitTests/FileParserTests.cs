using System.Text;
using FluentAssertions;
using HySite.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;

namespace HySite.Infrastructure.Tests;

public class FileParserTests
{
    [Fact]
    public void FileParserSuccessfully()
    {
        var fileProviderMock = new Mock<IFileProvider>();
        var loggerMock = new Mock<ILogger<FileParserService>>();
        var service = new FileParserService(fileProviderMock.Object, loggerMock.Object);

        string content = @"this is the title
            2015/01/20 17:00
            code,life
            @@@
            actual text";

        // Convert string to bytes
        byte[] byteArray = Encoding.UTF8.GetBytes(content);
        using MemoryStream stream = new(byteArray);
        using StreamReader reader = new(stream);

        var result = service.ParseFile("file.md", reader);
        result.IsSuccessful.Should().BeTrue();
        var dto = result.Value;
        dto.Title.Should().Be("this is the title");

        dto.Created.Should().HaveYear(2015);
        dto.Created.Should().HaveMonth(1);
        dto.Created.Should().HaveDay(20);

        dto.Tags.Should().Contain("code");
        dto.Tags.Should().Contain("life");
    }
    
}