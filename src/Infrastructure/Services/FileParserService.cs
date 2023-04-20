using System.Globalization;
using HySite.Application.Interfaces;
using HySite.Domain.Model;
using Markdig;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace HySite.Infrastructure.Services;

public class FileParserServiceException : Exception
{
    public FileParserServiceException() { }

    public FileParserServiceException(string message) : base(message) { }

    public FileParserServiceException(string message, Exception inner) : base(message, inner) { }
}

public class FileParserService : IFileParserService
{
    private IFileProvider _fileProvider;

    private readonly ILogger<FileParserService> _logger;

    public FileParserService(
        IFileProvider fileProvider,
        ILogger<FileParserService> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

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

    public IEnumerable<BlogPost> ParseExistingFiles(string path)
    {
        var start = DateTime.Now;

        List<IFileInfo> files = new ();
        LoadFiles(path, ref files);

        List<BlogPost> posts = new ();

        foreach (var fileInfo in files)
        {
            var fileName = fileInfo.Name;
            using var reader = new StreamReader(fileInfo.CreateReadStream());

            try
            {
                posts.Add(ParseFile(fileName, reader));
            }
            catch (FileParserServiceException ex)
            {
                _logger.LogWarning($"FileParserService.ParseExistingFiles Failed to parse file '{fileName}'. Error: {ex.Message}");
            }
        }

        var diff = (DateTime.Now - start).ToString();
        var count = posts.Count();
        _logger.LogInformation($"Parsing {count} posts, took {diff} time");
        return posts;
    }

    public BlogPost ParseFile(string fileName, StreamReader streamReader)
    {
        if (fileName.Contains(' '))
        {
            throw new FileParserServiceException($"Filename should not contain spaces");
        }

        var title = streamReader.ReadLine()?.Trim();
        if (title is null)
        {
            throw new FileParserServiceException($"File is empty");
        }

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
            throw new FileParserServiceException($"'{timeStr}' is not in the correct date format '{dateFormat}'");
        }

        var unusedMetaDataLine = streamReader.ReadLine()?.Trim();
        while (unusedMetaDataLine != "@@@")
        {
            if (streamReader.EndOfStream)
            {
                throw new FileParserServiceException("Metadata marker not found");
            }

            unusedMetaDataLine = streamReader.ReadLine()?.Trim();
        }

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            //.UseSyntaxHighlighting() //TODO: markdig syntax highlight is very outdated
            .UseFootnotes()
            .Build();

        var mdContent = streamReader.ReadToEnd();
        var htmlContent = Markdown.ToHtml(mdContent, pipeline);

        return new BlogPost()
        {
            FileName = Path.GetFileNameWithoutExtension(fileName).ToLower(),
            Title = title,
            MdContent = mdContent,
            HtmlContent = htmlContent,
            Created = postCreated
        };
    }
}


