using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace hySite
{
    public class AdminPageModel : PageModel
    {
        private IFileParserService _fileParserService;
        private IFileProvider _fileProvider;
        private IBlogPostRepository _blogPostRepository;
        private AppDbContext _dbContext;

        public string UploadError = "";
        public List<string> ExistingFileNames = new List<string>();

        [Required]
        [Display(Name="Select files to upload")]
        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        public AdminPageModel(IFileProvider fileProvider, IFileParserService fileParserService, IBlogPostRepository blogPostRepository, AppDbContext dbContext)
        {
            _fileParserService = fileParserService;
            _fileProvider = fileProvider;
            _blogPostRepository = blogPostRepository;
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            ReloadFilesList();
        }        

        public async Task<IActionResult> OnPostDeleteAsync(string fileName)
        {
            System.IO.File.Delete(Path.Combine("posts", fileName));
            ReloadFilesList();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadFileAsync()
        {
            if (!ModelState.IsValid)
            {
                UploadError = "Model state is invalid";
                return Page();
            }

            
            var fileName = UploadedFile?.FileName;

            if(fileName == null)
            {
                UploadError = "Failed to upload";
                return Page();
            }

            // If file is post, process it properly
            if(fileName.EndsWith(".md"))
            {
                // Read file content
                var reader = new StreamReader(
                    UploadedFile.OpenReadStream(), 
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), 
                    detectEncodingFromByteOrderMarks: true);

                // Try to parse content as BlogPost
                BlogPost blogPost;
                try 
                {
                    blogPost = _fileParserService.ParseFile(fileName, reader);
                }
                catch(FileParserServiceException ex)
                {  
                    UploadError = ex.Message;
                    ReloadFilesList();
                    return Page();
                }
                _blogPostRepository.Add(blogPost);
                await _dbContext.SaveChangesAsync();
            }

            // Save file to disk
            using (var stream = new FileStream(Path.Combine("posts", UploadedFile.FileName), FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            ReloadFilesList();
            return Page();
        }

        public async Task<IActionResult> OnPostRestartDBAsync()
        {
            _fileParserService.ParseExistingFiles();
            ReloadFilesList();
            return Page();
        }

        private void ReloadFilesList()
        {
            IDirectoryContents contents = _fileProvider.GetDirectoryContents("posts");
            IEnumerable<IFileInfo> files = contents.Where(f => !f.IsDirectory).OrderBy(f => f.LastModified);
            this.ExistingFileNames = files.Select( fi => fi.Name).ToList();
        }
    }
}