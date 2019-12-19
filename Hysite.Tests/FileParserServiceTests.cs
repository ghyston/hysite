using System;
using System.Text;
using System.IO;
using Xunit;
using Moq;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace hySite.Tests
{
    public class FileParserServiceTest
    { 
        private FileParserService _service {get; set; }

        private Mock<IFileProvider> MockFileProvider = new Mock<IFileProvider>();
        private Mock<IBlogPostRepository> MockBlogRepository = new Mock<IBlogPostRepository>();

        private Mock<ILogger<FileParserService>> MockLogger = new Mock<ILogger<FileParserService>>();

        private Mock<IConfiguration> MockConfig = new Mock<IConfiguration>();

        private AppDbContext _dbContext {get; set;}

        public FileParserServiceTest()
        {
             var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseInMemoryDatabase("FileParserServiceTestDb");
            _dbContext = new AppDbContext(optionsBuilder.Options);

            _service = new FileParserService(
                MockFileProvider.Object,
                _dbContext,
                MockBlogRepository.Object,
                MockLogger.Object,
                MockConfig.Object
            );
        }

        public StreamReader CreateStringReader(string fileContent)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContent);
            MemoryStream stream = new MemoryStream(byteArray);

            return new StreamReader(stream);
        }
        

        [Fact]
        public void EmptyFileTest()
        {
            var fileName = "";
            var fileContent = "";
            var reader = CreateStringReader(fileContent);
            Exception ex = Assert.Throws<FileParserServiceException>(() => _service.ParseFile(fileName, reader));
            Assert.Equal(ex.Message, "File is empty");
            //MockBlogRepository.Verify(br => br.Add(It.IsAny<BlogPost>()), Times.Never);
        }

        [Fact]
        public void DateIsStringTest()
        {
            var fileName = "";
            var fileContent = @"hohoho
        gogog";
            var reader = CreateStringReader(fileContent);
            Exception ex = Assert.Throws<FileParserServiceException>(() => _service.ParseFile(fileName, reader));
            Assert.StartsWith("'gogog' is not in the correct date format", ex.Message); 
        }

        [Fact]
        public void DateWrongFormatTest()
        {
            var fileName = "";
            var fileContent = @"hohoho
        12/12/2013";
            var reader = CreateStringReader(fileContent);
            Exception ex = Assert.Throws<FileParserServiceException>(() => _service.ParseFile(fileName, reader));
            Assert.StartsWith("'12/12/2013' is not in the correct date format", ex.Message); 
        }

        [Fact]
        public void HappySimple()
        {
            var fileName = "";
            var fileContent = @"hohoho
        2013/01/02 06:55
        @@@
post text";
            var reader = CreateStringReader(fileContent);
            var post = _service.ParseFile(fileName, reader);

            Assert.Equal(post.Title, "hohoho"); 
            Assert.Equal(post.Created.Year, 2013);
            Assert.Equal(post.Created.Month, 1);
            Assert.Equal(post.Created.Day, 2);
            Assert.Equal(post.Created.Hour, 6);
            Assert.Equal(post.Created.Minute, 55);
            Assert.Equal(post.MdContent, "post text");
        }

        [Fact]
        public void UnknownMetadataIgnored()
        {
            var fileName = "";
            var fileContent = @"hohoho
        2013/01/02 06:55
        This post is aboyt something important. Probably
        <SOME ADDITIONAL INFO>
        @@@
post text";
            var reader = CreateStringReader(fileContent);
            var post = _service.ParseFile(fileName, reader);

            Assert.Equal(post.Title, "hohoho"); 
            Assert.Equal(post.Created.Year, 2013);
            Assert.Equal(post.Created.Month, 1);
            Assert.Equal(post.Created.Day, 2);
            Assert.Equal(post.Created.Hour, 6);
            Assert.Equal(post.Created.Minute, 55);
            Assert.Equal(post.MdContent, "post text");
        }

        [Fact]
        public void MetadataMarkerNotFound()
        {
            var fileName = "";
            var fileContent = @"hohoho
        2013/01/02 06:55        
post text";
            var reader = CreateStringReader(fileContent);

            Exception ex = Assert.Throws<FileParserServiceException>(() => _service.ParseFile(fileName, reader));
            Assert.Equal(ex.Message, "Metadata marker not found"); 
        }

        [Fact]
        public void EmptyPostIsAlsoPost()
        {
            var fileName = "";
            var fileContent = @"hohoho
        2013/12/02 06:55
        This post is aboyt something important. Probably
        <SOME ADDITIONAL INFO>
        @@@";
            var reader = CreateStringReader(fileContent);
            var post = _service.ParseFile(fileName, reader);

            Assert.Equal(post.Title, "hohoho"); 
            Assert.Equal(post.Created.Year, 2013);
            Assert.Equal(post.Created.Month, 12);
            Assert.Equal(post.Created.Day, 2);
            Assert.Equal(post.Created.Hour, 6);
            Assert.Equal(post.Created.Minute, 55);
            Assert.Equal(post.MdContent, "");
            Assert.Equal(post.HtmlContent, "");
        }

        [Fact]
        public void FileNameContainSpaces()
        {
            var fileName = "some file";
            var fileContent = "";
            var reader = CreateStringReader(fileContent);
            Exception ex = Assert.Throws<FileParserServiceException>(() => _service.ParseFile(fileName, reader));
            Assert.Equal(ex.Message, "Filename should not contain spaces"); 
        }
    }
}
