using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hySite 
{
    public class GitController : Controller 
    {
        private readonly IGitRepository _gitRepository;
        private readonly IFileParserService _fileParserService;

        public GitController(IGitRepository gitRepository, IFileParserService fileParserService)
        {
            _gitRepository = gitRepository;
            _fileParserService = fileParserService;
        }

        [Route("update")]
        public IActionResult UpdatePosts()
        {
            _gitRepository.Pull();
            _fileParserService.ParseExistingFiles();
            return Ok();
        }
    }
}

