using HySite.Domain.Model;
using Microsoft.EntityFrameworkCore;

public interface IHysiteContext
{
    public DbSet<BlogPost> BlogPosts { get; }
    public DbSet<ViewStatistic> ViewStatistics { get; }
}