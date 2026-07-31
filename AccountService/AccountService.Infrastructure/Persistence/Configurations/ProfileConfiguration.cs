using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : EntityConfiguration<Profile>
{
    private const string ProfileIdColumn = "profile_id";

    public override void Configure(EntityTypeBuilder<Profile> builder)
    {
        base.Configure(builder);

        builder.ToTable("profiles");

        builder.Property(profile => profile.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(profile => profile.UserId).IsUnique().HasDatabaseName("ux_profiles_user_id");

        builder.Property(profile => profile.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(DisplayName.MaxLength)
            .HasConversion(displayName => displayName.Value, value => DisplayName.Create(value).Value)
            .IsRequired();

        builder.Property(profile => profile.Slug)
            .HasColumnName("slug")
            .HasMaxLength(ProfileSlug.MaxLength)
            .HasConversion(slug => slug.Value, value => ProfileSlug.Create(value).Value)
            .IsRequired();

        builder.HasIndex(profile => profile.Slug).IsUnique().HasDatabaseName("ux_profiles_slug");

        builder.Property(profile => profile.Bio).HasColumnName("bio").HasMaxLength(Profile.MaxBioLength);

        builder.Property(profile => profile.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(CountryCode.Length)
            .HasConversion(countryCode => countryCode!.Value, value => CountryCode.Create(value).Value);

        builder.Property(profile => profile.Visibility)
            .HasColumnName("visibility")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        ConfigureAvatar(builder);
        ConfigureStats(builder);
    }

    private static void ConfigureAvatar(EntityTypeBuilder<Profile> builder)
    {
        builder.OwnsOne(profile => profile.Avatar, avatar =>
        {
            avatar.Property(value => value.PublicId).HasColumnName("avatar_public_id").HasMaxLength(255).IsRequired();
            avatar.Property(value => value.SecureUrl).HasColumnName("avatar_url").HasMaxLength(500).IsRequired();
        });

        builder.Navigation(profile => profile.Avatar).IsRequired(false);
    }

    private static void ConfigureStats(EntityTypeBuilder<Profile> builder)
    {
        builder.OwnsOne(profile => profile.Stats, stats =>
        {
            stats.ToTable("profile_stats");
            stats.WithOwner().HasForeignKey(ProfileIdColumn);
            stats.Property<Guid>(ProfileIdColumn).HasColumnName(ProfileIdColumn);
            stats.HasKey(ProfileIdColumn);

            // Motivo: dos eventos de ascensión del mismo perfil se consumen en paralelo y ambos
            // recalculan desde el mismo estado; el token hace fallar al segundo para que reintente.
            stats.Property(value => value.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired()
                .IsConcurrencyToken();

            stats.Ignore(value => value.Overall);
            stats.Ignore(value => value.Public);

            ConfigureOverallStats(stats);
            ConfigurePublicStats(stats);
        });

        builder.Navigation(profile => profile.Stats).IsRequired();
    }

    private static void ConfigureOverallStats(OwnedNavigationBuilder<Profile, ProfileStats> stats)
    {
        stats.Property(value => value.TotalAscents).HasColumnName("total_ascents").IsRequired();
        stats.Property(value => value.DistinctPeaks).HasColumnName("distinct_peaks").IsRequired();
        stats.Property(value => value.HighestAltitudeMeters).HasColumnName("highest_altitude_m").IsRequired();
        stats.Property(value => value.HighestPeakId).HasColumnName("highest_peak_id");
        stats.Property(value => value.HighestPeakName)
            .HasColumnName("highest_peak_name").HasMaxLength(PeakSnapshot.MaxNameLength);
        stats.Property(value => value.LastAscentDate).HasColumnName("last_ascent_date");
    }

    private static void ConfigurePublicStats(OwnedNavigationBuilder<Profile, ProfileStats> stats)
    {
        stats.Property(value => value.PublicTotalAscents).HasColumnName("public_total_ascents").IsRequired();
        stats.Property(value => value.PublicDistinctPeaks).HasColumnName("public_distinct_peaks").IsRequired();
        stats.Property(value => value.PublicHighestAltitudeMeters)
            .HasColumnName("public_highest_altitude_m").IsRequired();
        stats.Property(value => value.PublicHighestPeakId).HasColumnName("public_highest_peak_id");
        stats.Property(value => value.PublicHighestPeakName)
            .HasColumnName("public_highest_peak_name").HasMaxLength(PeakSnapshot.MaxNameLength);
        stats.Property(value => value.PublicLastAscentDate).HasColumnName("public_last_ascent_date");
    }
}
