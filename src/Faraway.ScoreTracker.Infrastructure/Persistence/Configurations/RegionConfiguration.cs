using System.Text.Json;
using Faraway.ScoreTracker.Core.Enums;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<RegionRecord>
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    private static ValueComparer<List<NatureWonder>> ListEnumComparer =>
        new((a,b) => a!.SequenceEqual(b!),
            a => a!.Aggregate(0, (acc,x) => HashCode.Combine(acc, x.GetHashCode())),
            a => a!.ToList());
    
    public void Configure(EntityTypeBuilder<RegionRecord> b)
    {
        b.HasKey(x => x.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        
        b.OwnsOne(x => x.ScoringRule, sb =>
        {
            sb.Property(p => p.Type).IsRequired();
            sb.Property(p => p.Points).IsRequired();
        });

        b.Property(x => x.Wonders)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<NatureWonder>>(v, JsonOpts) ?? new())
            .Metadata.SetValueComparer(ListEnumComparer);

        b.Property(x => x.Condition)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<NatureWonder>>(v, JsonOpts) ?? new())
            .Metadata.SetValueComparer(ListEnumComparer);
    }
}
