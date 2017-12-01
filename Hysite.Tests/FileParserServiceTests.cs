using System;
using System.Text;
using System.IO;
using Xunit;
using Moq;
using Microsoft.Extensions.FileProviders;

namespace hySite.Tests
{
    public class FileParserServiceTest
    {
        private string Example1 = @"hohoho
        gogog
        @@@@";
        
        private FileParserService service {get; set; }

        private Mock<IFileProvider> MockFileProvider = new Mock<IFileProvider>();
        private Mock<AppDbContext> MockAppContextDb = new Mock<AppDbContext>();
        private Mock<IBlogPostRepository> MockBlogRepository = new Mock<IBlogPostRepository>();

        public FileParserServiceTest()
        {
            service = new FileParserService(
                MockFileProvider.Object,
                MockAppContextDb.Object,
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

            Assert.Throws<FileParserServiceException>(() => service.AddPostFromStream(fileName, reader));
            //Assert.True(true); 
        }
    }
}
