using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging.File;

namespace hySite
{
    //@todo: update handler register system!
    using INewFileHandler = IHandler<OnNewFileRequest, OnNewFileResponse>;
    using IRenameHandler = IHandler<OnFileRenamedRequest, OnFileRenamedResponse>;
    using IDeleteHandler = IHandler<OnFileDeletedRequest, OnFileDeletedResponse>;
    using IUpdateHandler = IHandler<OnFileChangedRequest, OnFileChangedResponse>;

    public class FileWatcherService : IFileWatcherSingleton
    {
        private readonly IServiceProvider _serviceProvider;
        private FileSystemWatcher _watcher;

        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(IServiceProvider serviceProvider, ILogger<FileWatcherService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void StartWatch()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = "posts";
            _watcher.Filter = "*.md";
            _watcher.NotifyFilter = 
                NotifyFilters.LastAccess | 
                NotifyFilters.LastWrite  | 
                NotifyFilters.FileName | 
                NotifyFilters.DirectoryName;

            _watcher.Created += new FileSystemEventHandler(OnFileCreated);
            _watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
            _watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            _watcher.EnableRaisingEvents = true;
        }

        private void OnFileCreated(object source, FileSystemEventArgs args)
        {
            _logger.LogInformation($"FileWatcherService.OnFileRenamed File {args.FullPath} created");
            try 
            {
                var handler = _serviceProvider.GetService<INewFileHandler>();
                (handler as INewFileHandler)?
                .Handle(new OnNewFileRequest {
                    FilePath = args.FullPath
                });
            }
            catch(Exception e)
            {
                _logger.LogError("FileWatcherService.OnFileCreated Error: " + e.Message);
            }
        }

        private void OnFileChanged(object source, FileSystemEventArgs args)
        {
            _logger.LogInformation($"FileWatcherService.OnFileRenamed File {args.FullPath} changed");
            try 
            {
                var handler = _serviceProvider.GetService<IUpdateHandler>();
                (handler as IUpdateHandler)?
                .Handle(new OnFileChangedRequest{
                    FilePath = args.FullPath
                });
            }
            catch(Exception e)
            {
                _logger.LogError("FileWatcherService.OnFileChanged Error: " + e.Message);
            }
        }

        private void OnFileDeleted(object source, FileSystemEventArgs args)
        {
            _logger.LogInformation($"FileWatcherService.OnFileRenamed File {args.FullPath} deleted");
            try 
            {
                var handler = _serviceProvider.GetService<IDeleteHandler> ();
                (handler as IDeleteHandler)?
                .Handle(new OnFileDeletedRequest{
                    FilePath = args.FullPath
                });
            }
            catch(Exception e)
            {
                _logger.LogError("FileWatcherService.OnFileDeleted Error: " + e.Message);
            }
        }

        private void OnFileRenamed(object source, RenamedEventArgs args)
        {
            _logger.LogInformation($"FileWatcherService.OnFileRenamed rename file from {args.OldFullPath} to {args.FullPath}");
            try 
            {
                var handler = _serviceProvider.GetService<IRenameHandler> ();
                (handler as IRenameHandler)?
                .Handle(new OnFileRenamedRequest{
                    OldFilePath = args.OldFullPath,
                    NewFilePath = args.FullPath
                });
            }
            catch(Exception e)
            {
                _logger.LogError("FileWatcherService.OnFileRenamed Error: " + e.Message);
            }
        }
    }
}