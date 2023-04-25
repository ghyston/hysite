using System.ComponentModel.DataAnnotations;

namespace HySite.Domain.Model;

public class BlogPost
{
    // TODO: remove redundant attributes
    public int Id { get; set; }

    [Required, StringLength(100)]
    public required string FileName { get; set; }

    [Required, StringLength(100)]
    public required string Title { get; set; }

    [Required]
    public required string MdContent { get; set; }

    [Required]
    public required string HtmlContent { get; set; }

    [Required]
    public DateTime Created {get; set;}

    public void Update(BlogPost post)
    {
        Title = post.Title;
        MdContent = post.MdContent;
        HtmlContent = post.HtmlContent;
    }
}