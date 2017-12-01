using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Markdig;

namespace hySite
{
    public class FileParserServiceException : Exception 
    { 
        public FileParserServiceException() { }

        public FileParserServiceException(string message) : base(message) { }

        public FileParserServiceException(string message, Exception inner) : base(message, inner) {} 
    }

    public class FileParserService : IFileParserService
    {
        private IFileProvider _fileProvider;
        private AppDbContext _dbContext;
        private IBlogPostRepository _blogPostRepository;

        public FileParserService(
            IFileProvider fileProvider, 
            AppDbContext db,
            IBlogPostRepository blogPostRepository)
        {
            _fileProvider = fileProvider;
            _dbContext = db;
            _blogPostRepository = blogPostRepository;
        }

        public void CreateDb()
        {
            IDirectoryContents contents = _fileProvider.GetDirectoryContents("posts");
            IEnumerable<IFileInfo> files = contents.Where(f => f.Name.EndsWith(".md") && !f.IsDirectory).OrderBy(f => f.LastModified);

            foreach(var fileInfo in files)
            {
                AddPostFromStream(fileInfo.Name, new StreamReader(fileInfo.CreateReadStream()));
            }
            _dbContext.SaveChanges();

        }



        public void AddPostFromStream(string fileName, StreamReader streamReader)
        {
            var title = streamReader.ReadLine();
            var timeStr = streamReader.ReadLine();
            DateTime postCreated = DateTime.Parse(timeStr); //@todo: do format provider, YYYY/mm/dd HH:mm
            var unusedMetaDataLine = streamReader.ReadLine();
            while(unusedMetaDataLine != "@@@")
            {
                if(streamReader.EndOfStream)
                {
                    throw new FileParserServiceException("Metadata marker not found");
                }

                //@todo: add test, if file is broken
                unusedMetaDataLine = streamReader.ReadLine();
            }

            var mdContent = streamReader.ReadToEnd();
            var htmlContent = Markdown.ToHtml(mdContent);

            BlogPost post = new BlogPost()
            {
                FileName = Path.GetFileNameWithoutExtension(fileName).ToLower(),
                Title = title,
                MdContent = mdContent,
                HtmlContent = htmlContent,
                Created = postCreated
            };
            _blogPostRepository.Add(post);
        }
    }

}

