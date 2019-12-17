using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace hySite 
{
    public class GitController : Controller 
    {
        private readonly IGitRepository _gitRepository;
        private readonly IFileParserService _fileParserService;
        private readonly IConfiguration _configuration;

        public GitController(IGitRepository gitRepository, IFileParserService fileParserService, IConfiguration configuration)
        {
            _gitRepository = gitRepository;
            _fileParserService = fileParserService;
            _configuration = configuration;
        }

        [Route("update")]
        public async Task<IActionResult> UpdatePostsAsync()
        {
            var signature = Request.Headers["X-Hub-Signature"];
            
            using (var reader = new StreamReader(Request.Body))
            {
                var payload = await reader.ReadToEndAsync();
                if (_gitRepository.IsSecretValid(signature, payload))
                {
                    _gitRepository.Pull();
                    _fileParserService.ParseExistingFiles();
                    return Ok();
                }
            }

            return Unauthorized();
        }
    }
}

