using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Configurations;

public class ShrineConfiguration : IEntityTypeConfiguration<ShrineRecord>
{
    public void Configure(EntityTypeBuilder<ShrineRecord> b)
    {
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        
        b.HasOne(x => x.Player)
            .WithMany(p => p.Shrines)
            .HasForeignKey(x => x.PlayerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.OwnsOne(x => x.ScoringRule, sr =>
        {
            sr.Property(p => p.Type).IsRequired().HasColumnName("ScoringRule_Type");
            sr.Property(p => p.Points).IsRequired().HasColumnName("ScoringRule_Points");
            sr.Property(p => p.ColorOne).HasColumnName("ScoringRule_ColorOne");
            sr.Property(p => p.ColorTwo).HasColumnName("ScoringRule_ColorTwo");
        });
    }
}
