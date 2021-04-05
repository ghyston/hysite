using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace hySite
{
    public class RssController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IRssFeedService rssFeedService;

        public RssController(IConfiguration configuration, IRssFeedService rssFeedService)
        {
            this.configuration = configuration;
            this.rssFeedService = rssFeedService;
        }

        [ResponseCache(Duration=60)]
        [HttpGet("/rss")]
        
        public FileStreamResult Index()
        {
            var rssPath = String.Join('/', configuration["PostsLocalPath"], configuration["RssFeedFile"]);
            if(!System.IO.File.Exists(rssPath))
                rssFeedService.CreateRssFeed();

            var stream = System.IO.File.OpenRead(rssPath);
            return File(stream, "application/rss+xml; charset=utf-8");
        }

    }
}