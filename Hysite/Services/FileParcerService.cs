using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Markdig;
using Markdig.SyntaxHighlighting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
        private IConfiguration _configuration;

        private readonly ILogger<FileParserService> _logger;

        public FileParserService(
            IFileProvider fileProvider, 
            AppDbContext db,
            IBlogPostRepository blogPostRepository,
            ILogger<FileParserService> logger,
            IConfiguration configuration)
        {
            _fileProvider = fileProvider;
            _dbContext = db;
            _blogPostRepository = blogPostRepository;
            _logger = logger;
            _configuration = configuration;
        }

        private void LoadFiles(string path, ref List<IFileInfo> result)
        {
            var contents = _fileProvider.GetDirectoryContents(path);
            var filesDirectly = contents.Where(f => f.Name.EndsWith(".md") && !f.IsDirectory).OrderBy(f => f.LastModified).ToList();
            result.AddRange(filesDirectly);

            var subDirectories = contents.Where(f => f.IsDirectory);
            foreach(var subdir in subDirectories)
                LoadFiles(Path.Combine(path, subdir.Name), ref result);
        }

        public void ParseExistingFiles()
        {
            var start = DateTime.Now;

            var files = new List<IFileInfo>();
            LoadFiles(_configuration["PostsLocalPath"], ref files);

            List<BlogPost> posts = new List<BlogPost>();

            foreach(var fileInfo in files)
            {
                var fileName = fileInfo.Name;
                using(var reader = new StreamReader(fileInfo.CreateReadStream()))
                {
                    try {

                        var post = ParseFile(fileName, reader);
                        posts.Add(post);
                    }
                    catch(FileParserServiceException ex)
                    {
                        _logger.LogError($"FileParserService.ParseExistingFiles Failed to parse file '{fileName}'. Error: {ex.Message}");                        
                    }
                }
            }

            _blogPostRepository.RemoveAll();
            _blogPostRepository.Add(posts);
            _dbContext.SaveChanges();
            
            var diff = (DateTime.Now - start).ToString();
            var count = _dbContext.BlogPosts.Count();
            _logger.LogInformation($"Parsing {count} posts, took {diff} time");
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
            var dateFormat = "yyyy/MM/dd HH:mm";
            DateTime postCreated;
            try
            {
                postCreated = DateTime.ParseExact(timeStr, dateFormat, CultureInfo.InvariantCulture);
            }
            catch (FormatException) 
            {
                throw new FileParserServiceException($"'{timeStr}' is not in the correct date format '{dateFormat}'");
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

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseSyntaxHighlighting()
                .Build();
            var mdContent = streamReader.ReadToEnd();
            var htmlContent = Markdown.ToHtml(mdContent, pipeline);

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

