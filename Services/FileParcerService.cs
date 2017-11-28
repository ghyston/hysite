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
                AddFile(fileInfo);
            }
            _dbContext.SaveChanges();

        }

        public void AddFile(IFileInfo fileInfo)
        {
            var streamReader = new StreamReader(fileInfo.CreateReadStream());
            var title = streamReader.ReadLine();
            var timeStr = streamReader.ReadLine();
            DateTime postCreated = DateTime.Parse(timeStr); //@todo: do format provider, YYYY/mm/dd HH:mm
            var unusedMetaDataLine = streamReader.ReadLine();
            while(unusedMetaDataLine != "@@@")
            {
                //@todo: add test, if file is broken
                unusedMetaDataLine = streamReader.ReadLine();
            }

            var mdContent = streamReader.ReadToEnd();
            var htmlContent = Markdown.ToHtml(mdContent);

            BlogPost post = new BlogPost()
            {
                FileName = Path.GetFileNameWithoutExtension(fileInfo.Name).ToLower(),
                Title = title,
                MdContent = mdContent,
                HtmlContent = htmlContent,
                Created = postCreated
            };
            _blogPostRepository.Add(post);
        }
    }

}

