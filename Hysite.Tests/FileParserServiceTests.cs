using System;
using System.Text;
using System.IO;
using Xunit;
using Moq;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;

namespace hySite.Tests
{
    public class FileParserServiceTest
    {
        private string Example1 = @"hohoho
        gogog
        @@@@";
        
        private FileParserService _service {get; set; }

        private Mock<IFileProvider> MockFileProvider = new Mock<IFileProvider>();
        private Mock<IBlogPostRepository> MockBlogRepository = new Mock<IBlogPostRepository>();

        private AppDbContext _dbContext {get; set;}

        public FileParserServiceTest()
        {
             var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseInMemoryDatabase();
            _dbContext = new AppDbContext(optionsBuilder.Options);

            _service = new FileParserService(
                MockFileProvider.Object,
                _dbContext,
                MockBlogRepository.Object
            );
        }

        public StreamReader CreateStringReader(string fileContent)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(Example1);
            MemoryStream stream = new MemoryStream(byteArray);

            return new StreamReader(stream);
        }
        

        [Fact]
        public void EmptyFileTest()
        {
            var fileName = "";
            var fileContent = "";
            var reader = CreateStringReader(fileContent);
            //service.AddPostFromStream(fileName, reader);

            Assert.Throws<FileParserServiceException>(() => _service.AddPostFromStream(fileName, reader));
            //Assert.True(true); 
        }
    }
}
