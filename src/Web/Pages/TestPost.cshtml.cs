using System.Linq;
using HySite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HySite.Web.Pages
{
    public class TestPostModel(IFileParserService fileParserService, IBlogPostRepository blogPostRepository) : PageModel
    {
        

        const string Markdown = "Markdown\n\n This is just a main Text `rm -rf`\n\n``` cshapr\n\n int main(int charc, char** charv) => 0;\n\n```\n\nAnd example with class:\n\n```csharp\n\npublic class Foo {\n\n\tpublic int Bar { get; set;}\n\n}\n\n```";

        public string HtmlContent { get; set; }
        public string PostContent { get; set; }
        public string PostContentConverted { get; set; }

        private readonly IFileParserService _fileParserService = fileParserService;
        private readonly IBlogPostRepository _blogPostRepository = blogPostRepository;

        public IActionResult OnGet()
        {
            var lastPost = _blogPostRepository.RetrieveAll().Last();
            PostContent = lastPost.HtmlContent;
            PostContentConverted = _fileParserService.ConvertToHtml(lastPost.MdContent);
            HtmlContent = _fileParserService.ConvertToHtml(Markdown);
            return Page();
        }
    }
}