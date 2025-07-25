namespace HySite.Domain.Model;

public class BlogTag
{
    public required string Name { get; set; }
    public ICollection<BlogPost> BlogPosts { get; set; } = [];
}