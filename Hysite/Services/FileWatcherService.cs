using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;


namespace hySite
{

    using INewFileHandler = IHandler<OnNewFileRequest, OnNewFileResponse>;
    using IRenameHandler = IHandler<OnFileRenamedRequest, OnFileRenamedResponse>;
    using IDeleteHandler = IHandler<OnFileDeletedRequest, OnFileDeletedResponse>;
    using IUpdateHandler = IHandler<OnFileChangedRequest, OnFileChangedResponse>;

    public class FileWatcherService : IFileWatcherSingleton
    {
        private readonly IServiceProvider _serviceProvider;
        private FileSystemWatcher _watcher;

        public FileWatcherService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                Console.WriteLine("Error: " +  e.Message);
                //@todo: log this
            }
        }

        private void OnFileChanged(object source, FileSystemEventArgs args)
        {
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
                Console.WriteLine("Error: " +  e.Message);
                //@todo: log this
            }
        }

        private void OnFileDeleted(object source, FileSystemEventArgs args)
        {
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
                Console.WriteLine("Error: " +  e.Message);
                //@todo: log this
            }
        }

        private void OnFileRenamed(object source, RenamedEventArgs args)
        {
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
                Console.WriteLine("Error: " +  e.Message);
                //@todo: log this
            }
        }
    }
}