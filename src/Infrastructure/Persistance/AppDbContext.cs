using Microsoft.EntityFrameworkCore;
using HySite.Domain.Model;

namespace HySite.Infrastructure.Persistance;

public class AppDbContext : DbContext, IHysiteContext
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    {

    }

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ViewStatistic> ViewStatistics => Set<ViewStatistic>();
}