using HySite.Domain.Model;
using Microsoft.EntityFrameworkCore;

public interface IHysiteContext
{
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<ViewStatistic> ViewStatistics { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}