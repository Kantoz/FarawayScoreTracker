using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Configurations;

public class PlayerRegionConfiguration : IEntityTypeConfiguration<PlayerRegionRecord>
{
    public void Configure(EntityTypeBuilder<PlayerRegionRecord> b)
    {
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        
        b.HasOne(x => x.Player)
            .WithMany(p => p.PlayerRegions)
            .HasForeignKey(x => x.PlayerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
