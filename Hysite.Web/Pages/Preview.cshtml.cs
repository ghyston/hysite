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
    public class PreviewPageModel : PageModel
    {
        private IFileParserService _fileParserService { get; set; }

        public string UploadError = "";

        public BlogPost PostToPreview {get; set;}

        [Required]
        [Display(Name="Select file to preview")]
        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        public PreviewPageModel(IFileParserService fileParserService)
        {
            _fileParserService = fileParserService;
        }

        public IActionResult OnPostUploadFile()
        {
            if(!ModelState.IsValid)
            {
                UploadError = "Model state is invalid";
                return Page();
            }

            var fileName = UploadedFile?.FileName;

            var reader = new StreamReader(
                UploadedFile.OpenReadStream(), 
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), 
                detectEncodingFromByteOrderMarks: true);

            // Try to parse content as BlogPost
            try 
            {
                PostToPreview = _fileParserService.ParseFile(fileName, reader);
            }
            catch(FileParserServiceException ex)
            {  
                UploadError = ex.Message;
            }
            return Page();
         }


/*-        {
-
-            
-            var fileName = UploadedFile?.FileName;
-
-            if(fileName == null)
-            {
-                UploadError = "Failed to upload";
-                return Page();
-            }
-
-            // If file is post, process it properly
-            if(fileName.EndsWith(".md"))
-            {
-                // Read file content
-                var reader = new StreamReader(
-                    UploadedFile.OpenReadStream(), 
-                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), 
-                    detectEncodingFromByteOrderMarks: true);
-
-                // Try to parse content as BlogPost
-                BlogPost blogPost;
-                try 
-                {
-                    blogPost = _fileParserService.ParseFile(fileName, reader);
-                }
-                catch(FileParserServiceException ex)
-                {  
-                    UploadError = ex.Message;
-                    ReloadFilesList();
-                    return Page();
-                }
-                _blogPostRepository.Add(blogPost);
-                await _dbContext.SaveChangesAsync();
-            }
-
-            // Save file to disk
-            using (var stream = new FileStream(Path.Combine("posts", UploadedFile.FileName), FileMode.Create))
-            {
-                await UploadedFile.CopyToAsync(stream);
-            }
-
-            ReloadFilesList();
-            return Page();
-        }*/
    }

}

