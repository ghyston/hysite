using System;
using System.ComponentModel.DataAnnotations;

namespace HySite.Domain.Model;

public class BlogPost
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string MdContent { get; set; } = string.Empty;

    [Required]
    public string HtmlContent { get; set; } = string.Empty;

    [Required]
    public DateTime Created {get; set;}

    public void Update(BlogPost post)
    {
        Title = post.Title;
        MdContent = post.MdContent;
        HtmlContent = post.HtmlContent;
    }
}