namespace HySite.Domain.Dtos;

public record BlogPostDto
{
    public DateTime Created { get; set; }
    public string[] Tags { get; set; } = [];
    public required string Title { get; set; }
    public required string FileName { get; set; }
    public required string Content { get; set; }
}
