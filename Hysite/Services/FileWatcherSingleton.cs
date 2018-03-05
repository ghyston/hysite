using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;


namespace hySite
{
    public class FileWatcherSingleton : IFileWatcherSingleton
    {
        private readonly IServiceProvider _serviceProvider;

        public FileWatcherSingleton(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
        }

        private void OnFileCreated(object source, FileSystemEventArgs args)
        {
            Console.WriteLine("File: " +  args.FullPath + " " + args.ChangeType);

            var handler = _serviceProvider.GetService<IHandler<OnNewFileRequest, OnNewFileResponse>>();
            (handler as IHandler<OnNewFileRequest, OnNewFileResponse>)?
            .Handle(new OnNewFileRequest {
                FilePath = args.FullPath
            });
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