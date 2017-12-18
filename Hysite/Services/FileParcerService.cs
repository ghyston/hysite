using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
                var fileName = fileInfo.Name;
                var reader = new StreamReader(fileInfo.CreateReadStream());

                try {
                    var post = ParseFile(fileName, reader);
                    _blogPostRepository.Add(post);
                }
                catch(FileParserServiceException ex)
                {
                    //@todo: add logger
                    Console.WriteLine($"File: {fileName} Exception: {ex.Message}");
                }
            }
            _dbContext.SaveChanges();

        }



        public BlogPost ParseFile(string fileName, StreamReader streamReader)
        {
            if(fileName.Contains(' '))
            {
                throw new FileParserServiceException($"Filename should not contain spaces");
            }

            var title = streamReader.ReadLine()?.Trim();
            if(title is null)
            {
                throw new FileParserServiceException($"File is empty");
            }

            var timeStr = streamReader.ReadLine()?.Trim();
            DateTime postCreated;
            try
            {
                postCreated = DateTime.ParseExact(timeStr, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            }
            catch (FormatException fe) 
            {
                throw new FileParserServiceException($"{timeStr} is not in the correct date format: {fe.Message}");
            } 
            
            var unusedMetaDataLine = streamReader.ReadLine()?.Trim();
            while(unusedMetaDataLine != "@@@")
            {
                if(streamReader.EndOfStream)
                {
                    throw new FileParserServiceException("Metadata marker not found");
                }

                unusedMetaDataLine = streamReader.ReadLine()?.Trim();
            }

            var mdContent = streamReader.ReadToEnd();
            var htmlContent = Markdown.ToHtml(mdContent);

            return new BlogPost()
            {
                FileName = Path.GetFileNameWithoutExtension(fileName).ToLower(),
                Title = title,
                MdContent = mdContent,
                HtmlContent = htmlContent,
                Created = postCreated
            };
        }
    }

}

