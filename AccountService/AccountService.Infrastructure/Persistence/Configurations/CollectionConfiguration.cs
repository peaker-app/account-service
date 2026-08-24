using AccountService.Domain.Collections;
using AccountService.Domain.Profiles;
using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

internal sealed class CollectionConfiguration : EntityConfiguration<Collection>
{
    private const int KindLength = 20;
    private const string CaseAndAccentInsensitive = "utf8mb4_0900_ai_ci";
    private const string CollectionIdColumn = "collection_id";

    public override void Configure(EntityTypeBuilder<Collection> builder)
    {
        base.Configure(builder);

        builder.ToTable("collections");
        builder.Property(collection => collection.UpdatedAtUtc).IsConcurrencyToken();
        builder.Ignore(collection => collection.PeakCount);
        builder.Ignore(collection => collection.IsDefault);

        ConfigureColumns(builder);
        ConfigureRelationships(builder);
        ConfigurePeaks(builder);
    }

    private static void ConfigureColumns(EntityTypeBuilder<Collection> builder)
    {
        builder.Property(collection => collection.ProfileId).HasColumnName("profile_id").IsRequired();

        builder.Property(collection => collection.Kind)
            .HasColumnName("kind")
            .HasMaxLength(KindLength)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(collection => collection.Name)
            .HasColumnName("name")
            .HasMaxLength(CollectionName.MaxLength)
            .HasConversion(name => name.Value, value => CollectionName.Create(value).Value)
            .UseCollation(CaseAndAccentInsensitive)
            .IsRequired();

        builder.Property(collection => collection.Description)
            .HasColumnName("description")
            .HasMaxLength(Collection.MaxDescriptionLength);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Collection> builder)
    {
        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(collection => collection.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(collection => new { collection.ProfileId, collection.Name })
            .IsUnique()
            .HasDatabaseName("ux_collections_profile_name");
    }

    private static void ConfigurePeaks(EntityTypeBuilder<Collection> builder)
    {
        builder.OwnsMany(collection => collection.Peaks, peak =>
        {
            peak.ToTable("collection_peaks");
            peak.WithOwner().HasForeignKey(CollectionIdColumn);
            peak.HasKey(entity => entity.Id);

            peak.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
            peak.Property<Guid>(CollectionIdColumn).HasColumnName(CollectionIdColumn);
            peak.Property(entity => entity.PeakId).HasColumnName("peak_id").IsRequired();
            peak.Property(entity => entity.PeakName)
                .HasColumnName("peak_name").HasMaxLength(CollectionPeakSnapshot.MaxNameLength).IsRequired();
            peak.Property(entity => entity.PeakAltitudeMeters).HasColumnName("peak_altitude_m").IsRequired();
            peak.Property(entity => entity.AddedAtUtc).HasColumnName("added_at_utc").IsRequired();

            peak.HasIndex(CollectionIdColumn, nameof(CollectionPeak.PeakId))
                .IsUnique().HasDatabaseName("ux_collection_peaks_unique");
            peak.HasIndex(entity => entity.PeakId).HasDatabaseName("ix_collection_peaks_peak");
        });

        builder.Navigation(collection => collection.Peaks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
