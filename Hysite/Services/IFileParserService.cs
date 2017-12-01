using Microsoft.Extensions.FileProviders;
using System.IO;

namespace hySite
{
    public interface IFileParserService
    {
        //@todo: rename?
        void CreateDb();
        void AddPostFromStream(string fileName, StreamReader streamReader);
    }

}

