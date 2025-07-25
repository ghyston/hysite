using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HySite.Domain.Model;

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTag>
{
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.HasKey(c => c.Name);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);
    }
}