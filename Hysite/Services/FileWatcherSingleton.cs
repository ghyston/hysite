using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;


namespace hySite
{
    public class FileWatcherSingleton : IFileWatcherSingleton
    {
        private readonly IFileParserService _fileParserService;
        private readonly IBlogPostRepository _blogPostRepository;
        private AppDbContext _dbContext;

        public FileWatcherSingleton(IFileParserService fileParserService, IBlogPostRepository blogPostRepository, AppDbContext dbContext)
        {
            _fileParserService = fileParserService;
            _blogPostRepository = blogPostRepository;
            _dbContext = dbContext;
        }

        public void StartWatch()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = "posts";
            watcher.Filter = "*.md";
            watcher.NotifyFilter = 
                NotifyFilters.LastAccess | 
                NotifyFilters.LastWrite  | 
                NotifyFilters.FileName | 
                NotifyFilters.DirectoryName;

            watcher.Created += new FileSystemEventHandler(OnFileCreated);
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            watcher.EnableRaisingEvents = true;
            

            //@todo
        }

        private void OnFileCreated(object source, FileSystemEventArgs args)
        {
            Console.WriteLine("File: " +  args.FullPath + " " + args.ChangeType);
            
            var fileName = Path.GetFileName(args.FullPath);
            var fileInfo = new FileInfo(args.FullPath);

            using(var reader = fileInfo.OpenText())
            {
                try {
                    var blogPost = _fileParserService.ParseFile(fileName, reader);
                    _blogPostRepository.Add(blogPost);
                    _dbContext.SaveChanges();
                }
                catch(Exception e)
                {
                    //@todo: log it
                }
            }
        }

        static private void OnFileChanged(object source, FileSystemEventArgs args)
        {
            Console.WriteLine("File: " +  args.FullPath + " " + args.ChangeType);
            //@todo
        }

        static private void OnFileDeleted(object source, FileSystemEventArgs args)
        {
            Console.WriteLine("File: " +  args.FullPath + " " + args.ChangeType);
            //@todo
        }

        static private void OnFileRenamed(object source, RenamedEventArgs args)
        {
            Console.WriteLine("File: {0} renamed to {1}", args.OldFullPath, args.FullPath);
            //@todo
        }
    }
}