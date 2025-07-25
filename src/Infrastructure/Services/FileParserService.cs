using System.Globalization;
using HySite.Application.Interfaces;
using HySite.Domain.Common;
using HySite.Domain.Model;
using HySite.Domain.Dtos;
using Markdig;
using Markdown.ColorCode;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace HySite.Infrastructure.Services;

public class FileParserServiceException : Exception
{
    public FileParserServiceException() { }

    public FileParserServiceException(string message) : base(message) { }

    public FileParserServiceException(string message, Exception inner) : base(message, inner) { }
}

public class FileParserService(
    IFileProvider fileProvider,
    ILogger<FileParserService> logger) : IFileParserService
{
    private readonly IFileProvider _fileProvider = fileProvider;

    private readonly ILogger<FileParserService> _logger = logger;

    private void LoadFiles(string path, ref List<IFileInfo> result)
    {
        var contents = _fileProvider.GetDirectoryContents(path);
        var filesDirectly = contents
            .Where(f => f.Name.EndsWith(".md") && !f.IsDirectory)
            .OrderBy(f => f.LastModified)
            .ToList();
        result.AddRange(filesDirectly);

        var subDirectories = contents.Where(f => f.IsDirectory);
        foreach (var subdir in subDirectories)
            LoadFiles(Path.Combine(path, subdir.Name), ref result);
    }

    public IEnumerable<BlogPostDto> ParseExistingFiles(string path)
    {
        var start = DateTime.Now;

        List<IFileInfo> files = new ();
        LoadFiles(path, ref files);

        List<BlogPostDto> posts = new ();

        foreach (var fileInfo in files)
        {
            var fileName = fileInfo.Name;
            using var reader = new StreamReader(fileInfo.CreateReadStream());

            var parseResult = ParseFile(fileName, reader);

            if (!parseResult.IsSuccessful)
            {
                _logger.LogWarning($"FileParserService.ParseExistingFiles Failed to parse file '{fileName}'. Error: {parseResult.Message}");
                continue;
            }

            posts.Add(parseResult.Value);
        }

        var diff = (DateTime.Now - start).ToString();
        var count = posts.Count;
        _logger.LogInformation($"Parsing {count} posts, took {diff} time");
        return posts;
    }

    public Result<BlogPostDto> ParseFile(string fileName, StreamReader streamReader)
    {
        if (fileName.Contains(' '))
            return Result<BlogPostDto>.Error($"Filename should not contain spaces");

        var title = streamReader.ReadLine()?.Trim();
        if (title is null)
            return Result<BlogPostDto>.Error($"File is empty");

        var timeStr = streamReader.ReadLine()?.Trim() ?? string.Empty;
        var dateFormat = "yyyy/MM/dd HH:mm";
        DateTime postCreated;
        try
        {
            var parsedDate = DateTime.ParseExact(timeStr, dateFormat, CultureInfo.InvariantCulture);
            postCreated = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }
        catch (FormatException)
        {
            return Result<BlogPostDto>.Error($"'{timeStr}' is not in the correct date format '{dateFormat}'");
        }

        var unusedMetaDataLine = streamReader.ReadLine()?.Trim();

        string[] tags = [];

        while (unusedMetaDataLine != "@@@")
        {
            if (streamReader.EndOfStream)
                return Result<BlogPostDto>.Error("Metadata marker not found");

            if (tags.Count() == 0)
                tags = unusedMetaDataLine.Split(',')
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToArray();

            unusedMetaDataLine = streamReader.ReadLine()?.Trim();
        }

        var content = streamReader.ReadToEnd();

        return Result<BlogPostDto>.Success(
            new BlogPostDto()
            {
                Created = postCreated,
                Title = title,
                FileName = Path.GetFileNameWithoutExtension(fileName).ToLower(),
                Content = content,
                Tags = tags
            });
    }

    public string ConvertToHtml(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseColorCode()
            .UseFootnotes()
            .Build();

        return Markdig.Markdown.ToHtml(markdown, pipeline);
    }
}


