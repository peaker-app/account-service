using AccountService.Domain.Profiles;
using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : EntityConfiguration<Profile>
{
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
            stats.WithOwner().HasForeignKey("profile_id");
            stats.Property<Guid>("profile_id").HasColumnName("profile_id");
            stats.HasKey("profile_id");

            stats.Property(value => value.TotalAscents).HasColumnName("total_ascents").IsRequired();
            stats.Property(value => value.DistinctPeaks).HasColumnName("distinct_peaks").IsRequired();
            stats.Property(value => value.HighestAltitudeMeters).HasColumnName("highest_altitude_m").IsRequired();
            stats.Property(value => value.HighestPeakId).HasColumnName("highest_peak_id");
            stats.Property(value => value.HighestPeakName).HasColumnName("highest_peak_name").HasMaxLength(200);
            stats.Property(value => value.LastAscentDate).HasColumnName("last_ascent_date");
            stats.Property(value => value.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        });

        builder.Navigation(profile => profile.Stats).IsRequired();
    }
}
