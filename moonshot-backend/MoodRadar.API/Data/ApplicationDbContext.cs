namespace MoodRadar.API.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MoodRadar.API.Models.Domain;
using System.Text.Json;

/// <summary>
/// Entity Framework Core DbContext for MoodRadar application.
/// Manages connection to PostgreSQL database and entity mappings.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Database tables
    public DbSet<NeighborhoodSnapshot> NeighborhoodSnapshots => Set<NeighborhoodSnapshot>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Weather> Weathers => Set<Weather>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Quarter> Quarters => Set<Quarter>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NeighborhoodSnapshot configuration
        modelBuilder.Entity<NeighborhoodSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NeighborhoodId).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.MoodLabel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Confidence).IsRequired();
            var featureJsonProperty = entity.Property(e => e.FeatureJson)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => SerializeFeatures(v),
                    v => DeserializeFeatures(v)
                );

            // Add value comparer for change tracking
            featureJsonProperty.Metadata.SetValueComparer(
                new ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => CompareFeatures(c1, c2),
                    c => ComputeHashCode(c),
                    c => c != null ? new Dictionary<string, object>(c) : new Dictionary<string, object>()
                )
            );

            // Foreign key
            entity.HasOne<Neighborhood>()
                .WithMany()
                .HasForeignKey(e => e.NeighborhoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for fast lookups
            entity.HasIndex(e => new { e.NeighborhoodId, e.Timestamp });
        });

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.NeighborhoodId);
            entity.Property(e => e.CachedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for fast filtering
            entity.HasIndex(e => new { e.StartTime, e.NeighborhoodId });
            entity.HasIndex(e => e.StartTime);
        });

        // Weather configuration
        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SnapshotHour).IsRequired();
            entity.Property(e => e.TemperatureC).IsRequired();
            entity.Property(e => e.PrecipitationProbability).IsRequired();
            entity.Property(e => e.CloudCover).IsRequired();
            entity.Property(e => e.CachedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for time-based queries
            entity.HasIndex(e => e.SnapshotHour);
        });

        // District configuration
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GeoJsonBoundary).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasMany(e => e.Quarters).WithOne(q => q.District).OnDelete(DeleteBehavior.Cascade);
        });

        // Quarter configuration
        modelBuilder.Entity<Quarter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DistrictId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GeoJsonBoundary).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.District).WithMany(d => d.Quarters).HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Neighborhoods).WithOne(n => n.Quarter).OnDelete(DeleteBehavior.Cascade);
        });

        // Neighborhood configuration
        modelBuilder.Entity<Neighborhood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuarterId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GeoJsonBoundary).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Quarter).WithMany(q => q.Neighborhoods).HasForeignKey(e => e.QuarterId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static string SerializeFeatures(Dictionary<string, object>? features)
    {
        return features != null ? JsonSerializer.Serialize(features) : "{}";
    }

    private static Dictionary<string, object>? DeserializeFeatures(string json)
    {
        return !string.IsNullOrEmpty(json) ? JsonSerializer.Deserialize<Dictionary<string, object>>(json) : null;
    }

    private static bool CompareFeatures(Dictionary<string, object>? dict1, Dictionary<string, object>? dict2)
    {
        if (dict1 == null && dict2 == null) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;
        return dict1.SequenceEqual(dict2);
    }

    private static int ComputeHashCode(Dictionary<string, object>? dict)
    {
        if (dict == null) return 0;
        var hash = new HashCode();
        foreach (var kvp in dict)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }
}
