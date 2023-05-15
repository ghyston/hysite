using HySite.Domain.Model;

namespace HySite.Application.Interfaces;

public interface IRssFeedService
{
    void CreateRssFeed(IEnumerable<BlogPost> posts, string rssPath);
}