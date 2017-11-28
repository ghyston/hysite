using Microsoft.Extensions.FileProviders;

namespace hySite
{
    public interface IFileParserService
    {
        //@todo: rename?
        void CreateDb();
        void AddFile(IFileInfo fileInfo);
    }

}

