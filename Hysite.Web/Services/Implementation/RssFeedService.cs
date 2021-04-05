using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace hySite
{
    public class RssFeedService : IRssFeedService
    {
        private readonly IBlogPostRepository blogPostRepository;
        private readonly IConfiguration configuration;
        private readonly ILogger<RssFeedService> logger;

        public RssFeedService(IBlogPostRepository blogPostRepository, IConfiguration configuration, ILogger<RssFeedService> logger)
        {
            this.blogPostRepository = blogPostRepository;
            this.configuration = configuration;
            this.logger = logger;
        }

        public void CreateRssFeed()
        {
            SyndicationFeed feed = new SyndicationFeed("Hyston blog", "Mumblings about programblings", new Uri("https://hyston.blog/rss"), "hyston.blog", DateTime.Now);
            SyndicationPerson sp = new SyndicationPerson("ghyston@gmail.com (Ilja Stepanow)", "Ilja Stepanow", "https://hyston.blog");
            feed.Authors.Add(sp);

            var allPosts = blogPostRepository.RetrieveAll();
            List<SyndicationItem> items = new List<SyndicationItem>();
            
            foreach(var post in allPosts)
            {
                try
                {
                    TextSyndicationContent textContent = new TextSyndicationContent(post.HtmlContent);
                    SyndicationItem item = new SyndicationItem(post.Title, textContent, new Uri($"https://hyston.blog/{post.FileName}"), post.FileName, post.Created);
                    items.Add(item);
                }
                catch (System.Exception)
                {
                    logger.LogError($"Failed to add post {post.FileName} into rss");
                }
            }

            feed.Items = items;
            feed.Language = "en-us";
            feed.LastUpdatedTime = DateTime.Now;

            var rssPath = String.Join('/', configuration["PostsLocalPath"], configuration["RssFeedFile"]);
            XmlWriter rssWriter = XmlWriter.Create(rssPath);
            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
            rssFormatter.WriteTo(rssWriter);
            rssWriter.Close();
        }
    }
}