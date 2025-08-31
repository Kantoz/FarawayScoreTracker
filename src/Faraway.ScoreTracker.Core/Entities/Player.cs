using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Core.Entities;
public record Player : BaseEntity
{
    private const int MaxRegions = 8;
    public required string Name { get; init; }

    private readonly List<PlayerRegion> _regions = [];
    
    public IReadOnlyList<PlayerRegion> Regions => _regions.OrderBy(r => r.Position).ToList();

    private readonly List<Shrine> _shrines = [];
    public IReadOnlyList<Shrine> Shrines  => _shrines.AsReadOnly();
    
    public void AddRegionAppend(Region? region, bool isFaceUp = false)
    {
        ArgumentNullException.ThrowIfNull(region);
        if (_regions.Count >= MaxRegions)
        {
            throw new InvalidOperationException("Es dürfen maximal 8 Regionen in der Auslage sein.");
        }

        int nextPos = _regions.Count + 1;

        PlayerRegion playerRegion = new()
        {
            Id = Guid.NewGuid(),
            Region   = region,
            IsFaceUp = isFaceUp,
            Position = nextPos,
        };

        _regions.Add(playerRegion);
    }
    
    public void AddShrine(Shrine shrine)
    {
        if (shrine.Id == Guid.Empty)
        {
            shrine.Id = Guid.NewGuid();
        }

        _shrines.Add(shrine);
    }

    private IEnumerable<Region> VisibleRegions()
    {
        return _regions.Where(r => r.IsFaceUp).Select(r => r.Region);
    }

    private IEnumerable<Shrine> AllShrines()
    {
        return _shrines;
    }

    private int CountHints()
    {
        return VisibleRegions().Count(r => r.HasHint)
               + AllShrines().Count(s => s.HasHint);
    }

    private int CountNightSymbols()
    {
        return VisibleRegions().Count(r => r.Value == TimeValue.Nacht)
               + AllShrines().Count(s => s.Value == TimeValue.Nacht);
    }

    private int CountWonders(NatureWonder wonder)
    {
        return VisibleRegions().Sum(r => r.Wonders.Count(w => w == wonder))
               + AllShrines().Sum(s => s.Wonders.Count(w => w == wonder));
    }

    private int CountCardsOfColor(Color color)
    {
        return VisibleRegions().Count(r => r.Area == color)
               + AllShrines().Count(s => s.Area == color);
    }

    private int CountColorSet(params Color[] set)
    {
        return set.Select(CountCardsOfColor).DefaultIfEmpty(0).Min();
    }
    
    public int ComputePlayerScore()
    {
        try
        {
            foreach (PlayerRegion playerRegion in Regions)
            {
                playerRegion.FlipRegionFaceDown();
            }
            
            var orderedLinks = Regions.OrderBy(r => r.Position).ToList();
            var total        = 0;

            foreach (var playerRegion in orderedLinks.AsEnumerable().Reverse())
            {
                playerRegion.FlipRegion();

                total += EvaluateRule(
                    rule: playerRegion.Region.ScoringRule,
                    condition: playerRegion.Region.Condition?.ToArray()
                );
            }

            foreach (var shrine in Shrines)
            {
                total += EvaluateRule(
                    rule: shrine.ScoringRule,
                    condition: null
                );
            }

            return total;
        }
        catch (Exception)
        {
            Console.WriteLine("Error while computing player scores.");
            return 0;
        }
    }
    
    private bool IsConditionMetForRegion(IReadOnlyCollection<NatureWonder>? condition)
    {
        if (condition == null || !condition.Any())
        {
            return true;
        }
        
        bool returnValue = true;
        var amountWonderByWonder = condition
            .GroupBy(w => w)
            .Select(grp => new KeyValuePair<NatureWonder, int>(grp.Key, grp.Count()))
            .ToDictionary();

        foreach (KeyValuePair<NatureWonder,int> keyValuePair in amountWonderByWonder)
        {
            returnValue &= CountWonders(keyValuePair.Key) >= keyValuePair.Value;
        }
        return returnValue;
    }

    private int EvaluateRule(ScoringRule? rule, IReadOnlyCollection<NatureWonder>? condition)
    {
        if (rule == null)
        {
            return 0;
        }

        if (!IsConditionMetForRegion(condition: condition))
        {
            return 0;
        }
        
        return rule.Type switch
        {
            ScoringType.Flat            => rule.Points,
            ScoringType.PerHint         => CountHints() * rule.Points,
            ScoringType.PerBlueStone    => CountWonders(NatureWonder.Stein) * rule.Points,
            ScoringType.PerChimaere     => CountWonders(NatureWonder.Chimaere) * rule.Points,
            ScoringType.PerDistel       => CountWonders(NatureWonder.Distel) * rule.Points,
            ScoringType.PerNightSymbol  => CountNightSymbols() * rule.Points,
            ScoringType.PerColor        => (rule.ColorOne.HasValue ? CountCardsOfColor(rule.ColorOne.Value) : 0) * rule.Points,
            ScoringType.PerColorTwo     => ((rule.ColorOne.HasValue ? CountCardsOfColor(rule.ColorOne.Value) : 0)
                                           + (rule.ColorTwo.HasValue ? CountCardsOfColor(rule.ColorTwo.Value) : 0)) * rule.Points,
            ScoringType.PerColorSet     => CountColorSet(Color.Gelb, Color.Rot, Color.Gruen, Color.Blau) * rule.Points,
            _ => 0
        };
    }
}
