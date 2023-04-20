using HySite.Domain.Model;

namespace HySite.Application.Interfaces;

public interface IFileParserService
{
    IEnumerable<BlogPost> ParseExistingFiles(string path);
    BlogPost ParseFile(string fileName, StreamReader streamReader);
}