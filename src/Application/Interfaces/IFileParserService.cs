using HySite.Domain.Common;
using HySite.Domain.Model;
using HySite.Domain.Dtos;

namespace HySite.Application.Interfaces;

public interface IFileParserService
{
    IEnumerable<BlogPostDto> ParseExistingFiles(string path);
    Result<BlogPostDto> ParseFile(string fileName, StreamReader streamReader);
    string ConvertToHtml(string markdown);
}