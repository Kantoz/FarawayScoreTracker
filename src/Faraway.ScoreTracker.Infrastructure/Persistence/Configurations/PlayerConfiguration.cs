using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<PlayerRecord>
{
    public void Configure(EntityTypeBuilder<PlayerRecord> b)
    {
        b.HasKey(x => x.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        
        b.Property(x => x.Name).IsRequired();

        b.HasMany(p => p.PlayerRegions)
            .WithOne(r => r.Player)
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(p => p.Shrines)
            .WithOne(s => s.Player)
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.Name).IsUnique();
    }
}
