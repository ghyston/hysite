using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;

namespace hySite
{
    public class RssFeedService : IRssFeedService
    {
        private readonly IBlogPostRepository blogPostRepository;

        RssFeedService(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository;
        }

        public void CreateRssFeed()
        {
            SyndicationFeed feed = new SyndicationFeed("Hyston blog", "Mumblings about programblings", new Uri("hyston.blog/rss"), "hyston.blog", DateTime.Now);
            SyndicationPerson sp = new SyndicationPerson("ghyston@gmail.com", "Ilja Stepanow", "hyston.blog");
            feed.Authors.Add(sp);

            // Add a custom element.
            /*XmlDocument doc = new XmlDocument();
            XmlElement feedElement = doc.CreateElement("CustomElement");
            feedElement.InnerText = "Some text";
            feed.ElementExtensions.Add(feedElement);

            feed.Generator = "Sample Code";
            feed.Id = "FeedID";
            feed.ImageUrl = new Uri("http://server/image.jpg");*/
            

            var allPosts = blogPostRepository.RetrieveAll();
            List<SyndicationItem> items = new List<SyndicationItem>();
            
            foreach(var post in allPosts)
            {
                TextSyndicationContent textContent = new TextSyndicationContent(post.HtmlContent);
                SyndicationItem item = new SyndicationItem(post.Title, textContent, new Uri($"hyston.blog/{post.FileName}"), post.FileName, post.Created);
                items.Add(item);
            }

            feed.Items = items;

            feed.Language = "en-us";
            feed.LastUpdatedTime = DateTime.Now;

            XmlWriter rssWriter = XmlWriter.Create("rss.xml");
            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
            rssFormatter.WriteTo(rssWriter);
            rssWriter.Close();
        }
    }
}