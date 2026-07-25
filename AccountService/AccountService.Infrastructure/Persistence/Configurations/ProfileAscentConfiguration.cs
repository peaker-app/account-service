using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

internal sealed class ProfileAscentConfiguration : EntityConfiguration<ProfileAscent>
{
    private const int VisibilityLength = 20;

    public override void Configure(EntityTypeBuilder<ProfileAscent> builder)
    {
        base.Configure(builder);

        builder.ToTable("profile_ascents");
        builder.Ignore(ascent => ascent.IsPublic);

        ConfigureColumns(builder);
        ConfigureRelationships(builder);
        ConfigurePeakSnapshot(builder);
    }

    private static void ConfigureColumns(EntityTypeBuilder<ProfileAscent> builder)
    {
        builder.Property(ascent => ascent.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(ascent => ascent.AscentId).HasColumnName("ascent_id").IsRequired();
        builder.Property(ascent => ascent.AscentDate).HasColumnName("ascent_date").IsRequired();

        builder.Property(ascent => ascent.Visibility)
            .HasColumnName("visibility")
            .HasMaxLength(VisibilityLength)
            .HasConversion<string>()
            .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<ProfileAscent> builder)
    {
        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(ascent => ascent.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ascent => ascent.AscentId).IsUnique().HasDatabaseName("ux_profile_ascents_ascent");
        builder.HasIndex(ascent => ascent.ProfileId).HasDatabaseName("ix_profile_ascents_profile");
    }

    private static void ConfigurePeakSnapshot(EntityTypeBuilder<ProfileAscent> builder)
    {
        builder.OwnsOne(ascent => ascent.Peak, peak =>
        {
            peak.Property(snapshot => snapshot.PeakId).HasColumnName("peak_id").IsRequired();
            peak.Property(snapshot => snapshot.Name)
                .HasColumnName("peak_name").HasMaxLength(PeakSnapshot.MaxNameLength).IsRequired();
            peak.Property(snapshot => snapshot.AltitudeMeters).HasColumnName("peak_altitude_m").IsRequired();

            peak.HasIndex(snapshot => snapshot.PeakId).HasDatabaseName("ix_profile_ascents_peak");
        });

        builder.Navigation(ascent => ascent.Peak).IsRequired();
    }
}
