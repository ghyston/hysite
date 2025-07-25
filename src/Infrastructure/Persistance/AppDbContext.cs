using Microsoft.EntityFrameworkCore;
using HySite.Domain.Model;

namespace HySite.Infrastructure.Persistance;

public class AppDbContext : DbContext, IHysiteContext
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogTagConfiguration).Assembly);

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogTag> BlogTags => Set<BlogTag>();
    public DbSet<ViewStatistic> ViewStatistics => Set<ViewStatistic>();
}