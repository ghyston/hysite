using System.ServiceModel.Syndication;
using System.Xml;
using HySite.Application.Interfaces;
using HySite.Domain.Model;

namespace HySite.Infrastructure.Services;

public class RssFeedService : IRssFeedService
{
    public void CreateRssFeed(IEnumerable<BlogPost> posts, string rssPath)
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

        var items = posts.Select(post => new SyndicationItem(
            title: post.Title,
            new TextSyndicationContent(post.HtmlContent),
            new Uri($"https://hyston.blog/{post.FileName}"),
            post.FileName,
            post.Created));

        feed.Items = items;
        feed.Language = "en-us";
        feed.LastUpdatedTime = DateTime.Now;

        var rssWriter = XmlWriter.Create(rssPath);
        feed.SaveAsRss20(rssWriter);
        rssWriter.Close();
    }
}