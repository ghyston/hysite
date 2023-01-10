using Microsoft.EntityFrameworkCore;
using HySite.Domain.Model;

namespace HySite.Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    {

    }

    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<ViewStatistic> ViewStatistics { get; set; }
}