using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace hySite;

public class RssFeedService : IRssFeedService
{
    private readonly AppDbContext appDbContext;

    public RssFeedService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task CreateRssFeed(string rssPath, CancellationToken cancellationToken)
    {
        var feed = new SyndicationFeed(
            title: "Hyston blog", 
            description: "Mumblings about programblings", 
            feedAlternateLink: new Uri("https://hyston.blog/rss"), 
            id: "hyston.blog", 
            lastUpdatedTime: DateTime.Now);
        var person = new SyndicationPerson(
            email: "ghyston@gmail.com (Ilja Stepanow)", 
            name: "Ilja Stepanow", 
            uri: "https://hyston.blog");
        feed.Authors.Add(person);

        var allPosts = await appDbContext
             .BlogPosts
             .OrderByDescending(p => p.Created)
             .ToListAsync(cancellationToken);

        var items = allPosts.Select(post => new SyndicationItem(
            title: post.Title, 
            new TextSyndicationContent(post.HtmlContent), 
            new Uri($"https://hyston.blog/{post.FileName}"), 
            post.FileName, 
            post.Created));

        feed.Items = items;
        feed.Language = "en-us";
        feed.LastUpdatedTime = DateTime.Now;

        var rssWriter = XmlWriter.Create(rssPath);
        var rssFormatter = new Rss20FeedFormatter(feed);
        rssFormatter.WriteTo(rssWriter);
        rssWriter.Close();
    }
}