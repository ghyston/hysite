using Microsoft.EntityFrameworkCore;

namespace hySite
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<ViewStatistic> ViewStatistics { get; set; }
    }
}