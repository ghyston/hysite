

using System.Threading;
using System.Threading.Tasks;

namespace hySite; 

public interface IRssFeedService
{
    Task CreateRssFeed(string rssPath, CancellationToken cancellationToken);
}
