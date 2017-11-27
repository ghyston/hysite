using System;
using System.ComponentModel.DataAnnotations;

namespace hySite
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FileName { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string MdContent { get; set; }

        [Required]
        public string HtmlContent { get; set; }

        [Required]
        public DateTime Created {get; set;}
    }
}