using Microsoft.Extensions.FileProviders;
using System.IO;

namespace hySite
{
    public interface IFileParserService
    {
        void ParseExistingFiles();
        BlogPost ParseFile(string fileName, StreamReader streamReader);
    }

}

