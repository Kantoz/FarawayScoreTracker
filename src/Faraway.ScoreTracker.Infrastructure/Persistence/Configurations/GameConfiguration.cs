using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<GameRecord>
{
    public void Configure(EntityTypeBuilder<GameRecord> b)
    {
        b.HasKey(x => x.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        
        b.Property(x => x.PlayTime).IsRequired();
        
        b.HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}