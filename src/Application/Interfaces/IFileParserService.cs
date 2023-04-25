using HySite.Domain.Common;
using HySite.Domain.Model;

namespace HySite.Application.Interfaces;

public interface IFileParserService
{
    IEnumerable<BlogPost> ParseExistingFiles(string path);
    Result<BlogPost> ParseFile(string fileName, StreamReader streamReader);
}