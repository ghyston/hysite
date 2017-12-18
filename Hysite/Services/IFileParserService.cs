using Microsoft.Extensions.FileProviders;
using System.IO;

namespace hySite
{
    public interface IFileParserService
    {
        //@todo: rename?
        void CreateDb();
        BlogPost ParseFile(string fileName, StreamReader streamReader);
    }

}

