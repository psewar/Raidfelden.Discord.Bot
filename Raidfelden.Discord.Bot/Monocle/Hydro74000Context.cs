using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Hydro74000Context : DbContext
    {
        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<AlembicVersion> AlembicVersion { get; set; }
        public virtual DbSet<Common> Common { get; set; }
        public virtual DbSet<Forts> Forts { get; set; }
        public virtual DbSet<FortSightings> FortSightings { get; set; }
        public virtual DbSet<GymDefenders> GymDefenders { get; set; }
        public virtual DbSet<MysterySightings> MysterySightings { get; set; }
        public virtual DbSet<Parks> Parks { get; set; }
        public virtual DbSet<Pokestops> Pokestops { get; set; }
        public virtual DbSet<Raids> Raids { get; set; }
        public virtual DbSet<Sightings> Sightings { get; set; }
        public virtual DbSet<Spawnpoints> Spawnpoints { get; set; }
        public virtual DbSet<Weather> Weather { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.ToTable("accounts");

                entity.HasIndex(e => e.Binded)
                    .HasName("ix_accounts_binded");

                entity.HasIndex(e => e.Captchaed)
                    .HasName("ix_accounts_captchaed");

                entity.HasIndex(e => e.Hibernated)
                    .HasName("ix_accounts_hibernated");

                entity.HasIndex(e => e.Instance)
                    .HasName("ix_accounts_instance");

                entity.HasIndex(e => e.LastHibernated)
                    .HasName("ix_accounts_last_hibernated");

                entity.HasIndex(e => e.Level)
                    .HasName("ix_accounts_level");

                entity.HasIndex(e => e.Reason)
                    .HasName("ix_accounts_reason");

                entity.HasIndex(e => e.Username)
                    .HasName("ix_accounts_username_unique")
                    .IsUnique();

                entity.HasIndex(e => new { e.ReserveType, e.Instance, e.Hibernated, e.Created })
                    .HasName("ix_accounts_acquisition");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Binded)
                    .HasColumnName("binded")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Captchaed)
                    .HasColumnName("captchaed")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id")
                    .HasMaxLength(64);

                entity.Property(e => e.DeviceVersion)
                    .HasColumnName("device_version")
                    .HasMaxLength(20);

                entity.Property(e => e.Hibernated)
                    .HasColumnName("hibernated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Instance)
                    .HasColumnName("instance")
                    .HasMaxLength(32);

                entity.Property(e => e.LastHibernated)
                    .HasColumnName("last_hibernated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Model)
                    .HasColumnName("model")
                    .HasMaxLength(20);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(32);

                entity.Property(e => e.Provider)
                    .IsRequired()
                    .HasColumnName("provider")
                    .HasMaxLength(12);

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(12);

                entity.Property(e => e.Remove)
                    .HasColumnName("remove")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.ReserveType)
                    .HasColumnName("reserve_type")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<AlembicVersion>(entity =>
            {
                entity.HasKey(e => e.VersionNum);

                entity.ToTable("alembic_version");

                entity.Property(e => e.VersionNum)
                    .HasColumnName("version_num")
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Common>(entity =>
            {
                entity.ToTable("common");

                entity.HasIndex(e => e.Key)
                    .HasName("ix_common_key");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasColumnName("key")
                    .HasMaxLength(32);

                entity.Property(e => e.Val)
                    .HasColumnName("val")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<Forts>(entity =>
            {
                entity.ToTable("forts");

                entity.HasIndex(e => e.ExternalId)
                    .HasName("external_id")
                    .IsUnique();

                entity.HasIndex(e => e.Parkid)
                    .HasName("ix_forts_parkid");

                entity.HasIndex(e => e.WeatherCellId)
                    .HasName("ix_forts_weather_cell");

                entity.HasIndex(e => new { e.Lat, e.Lon })
                    .HasName("ix_coords");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExternalId)
                    .HasColumnName("external_id")
                    .HasMaxLength(35);

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.Park)
                    .HasColumnName("park")
                    .HasMaxLength(200);

                entity.Property(e => e.Parkid)
                    .HasColumnName("parkid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Sponsor)
                    .HasColumnName("sponsor")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(200);

                entity.Property(e => e.WeatherCellId).HasColumnName("weather_cell_id");

                entity.HasOne(d => d.ParkNavigation)
                    .WithMany(p => p.Forts)
                    .HasForeignKey(d => d.Parkid)
                    .HasConstraintName("forts_fk_parkid");

                entity.HasOne(d => d.WeatherCell)
                    .WithMany(p => p.Forts)
                    .HasPrincipalKey(p => p.S2CellId)
                    .HasForeignKey(d => d.WeatherCellId)
                    .HasConstraintName("forts_fk_cellid");
            });

            modelBuilder.Entity<FortSightings>(entity =>
            {
                entity.ToTable("fort_sightings");

                entity.HasIndex(e => e.LastModified)
                    .HasName("ix_fort_sightings_last_modified");

                entity.HasIndex(e => new { e.FortId, e.LastModified })
                    .HasName("fort_id_last_modified_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FortId)
                    .HasColumnName("fort_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GuardPokemonId)
                    .HasColumnName("guard_pokemon_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.IsInBattle)
                    .HasColumnName("is_in_battle")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SlotsAvailable)
                    .HasColumnName("slots_available")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Team).HasColumnName("team");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Fort)
                    .WithMany(p => p.FortSightings)
                    .HasForeignKey(d => d.FortId)
                    .HasConstraintName("fort_sightings_ibfk_1");
            });

            modelBuilder.Entity<GymDefenders>(entity =>
            {
                entity.ToTable("gym_defenders");

                entity.HasIndex(e => e.Created)
                    .HasName("ix_gym_defenders_created");

                entity.HasIndex(e => e.FortId)
                    .HasName("ix_gym_defenders_fort_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AtkIv)
                    .HasColumnName("atk_iv")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.BattlesAttacked)
                    .HasColumnName("battles_attacked")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BattlesDefended)
                    .HasColumnName("battles_defended")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cp)
                    .HasColumnName("cp")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DefIv)
                    .HasColumnName("def_iv")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.ExternalId).HasColumnName("external_id");

                entity.Property(e => e.FortId)
                    .HasColumnName("fort_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Move1)
                    .HasColumnName("move_1")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Move2)
                    .HasColumnName("move_2")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Nickname)
                    .HasColumnName("nickname")
                    .HasMaxLength(128);

                entity.Property(e => e.NumUpgrades)
                    .HasColumnName("num_upgrades")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.OwnerName)
                    .HasColumnName("owner_name")
                    .HasMaxLength(128);

                entity.Property(e => e.PokemonId)
                    .HasColumnName("pokemon_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.StaIv)
                    .HasColumnName("sta_iv")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Stamina)
                    .HasColumnName("stamina")
                    .HasColumnType("int(11)");

                entity.Property(e => e.StaminaMax)
                    .HasColumnName("stamina_max")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Team).HasColumnName("team");

                entity.HasOne(d => d.Fort)
                    .WithMany(p => p.GymDefenders)
                    .HasForeignKey(d => d.FortId)
                    .HasConstraintName("gym_defenders_ibfk_1");
            });

            modelBuilder.Entity<MysterySightings>(entity =>
            {
                entity.ToTable("mystery_sightings");

                entity.HasIndex(e => e.EncounterId)
                    .HasName("ix_mystery_sightings_encounter_id");

                entity.HasIndex(e => e.FirstSeen)
                    .HasName("ix_mystery_sightings_first_seen");

                entity.HasIndex(e => e.SpawnId)
                    .HasName("ix_mystery_sightings_spawn_id");

                entity.HasIndex(e => e.WeatherCellId)
                    .HasName("ix_mystery_weather_cell");

                entity.HasIndex(e => new { e.EncounterId, e.SpawnId })
                    .HasName("unique_encounter")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AtkIv).HasColumnName("atk_iv");

                entity.Property(e => e.Cp)
                    .HasColumnName("cp")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.DefIv).HasColumnName("def_iv");

                entity.Property(e => e.EncounterId).HasColumnName("encounter_id");

                entity.Property(e => e.FirstSeconds)
                    .HasColumnName("first_seconds")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.FirstSeen)
                    .HasColumnName("first_seen")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Form)
                    .HasColumnName("form")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.LastSeconds)
                    .HasColumnName("last_seconds")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Move1)
                    .HasColumnName("move_1")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Move2)
                    .HasColumnName("move_2")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.PokemonId)
                    .HasColumnName("pokemon_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.SeenRange)
                    .HasColumnName("seen_range")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.SpawnId)
                    .HasColumnName("spawn_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.StaIv).HasColumnName("sta_iv");

                entity.Property(e => e.WeatherBoostedCondition)
                    .HasColumnName("weather_boosted_condition")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.WeatherCellId).HasColumnName("weather_cell_id");

                entity.HasOne(d => d.WeatherCell)
                    .WithMany(p => p.MysterySightings)
                    .HasPrincipalKey(p => p.S2CellId)
                    .HasForeignKey(d => d.WeatherCellId)
                    .HasConstraintName("mystery_sightings_fk_cellid");
            });

            modelBuilder.Entity<Parks>(entity =>
            {
                entity.HasKey(e => e.Internalid);

                entity.ToTable("parks");

                entity.HasIndex(e => e.Instanceid)
                    .HasName("ix_parks_instance");

                entity.HasIndex(e => e.Internalid)
                    .HasName("ix_parks_internalid");

                entity.Property(e => e.Internalid)
                    .HasColumnName("internalid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Coords).HasColumnName("coords");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Instanceid)
                    .IsRequired()
                    .HasColumnName("instanceid")
                    .HasMaxLength(32);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(254);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Pokestops>(entity =>
            {
                entity.ToTable("pokestops");

                entity.HasIndex(e => e.ExternalId)
                    .HasName("external_id")
                    .IsUnique();

                entity.HasIndex(e => e.Lat)
                    .HasName("ix_pokestops_lat");

                entity.HasIndex(e => e.Lon)
                    .HasName("ix_pokestops_lon");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExternalId)
                    .HasColumnName("external_id")
                    .HasMaxLength(35);

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Raids>(entity =>
            {
                entity.ToTable("raids");

                entity.HasIndex(e => e.ExternalId)
                    .HasName("external_id")
                    .IsUnique();

                entity.HasIndex(e => e.FortId)
                    .HasName("fort_id");

                entity.HasIndex(e => e.TimeSpawn)
                    .HasName("ix_raids_time_spawn");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cp)
                    .HasColumnName("cp")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExternalId)
                    .HasColumnName("external_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.FortId)
                    .HasColumnName("fort_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Level).HasColumnName("level");

                entity.Property(e => e.Move1)
                    .HasColumnName("move_1")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Move2)
                    .HasColumnName("move_2")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.PokemonId)
                    .HasColumnName("pokemon_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.TimeBattle)
                    .HasColumnName("time_battle")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TimeEnd)
                    .HasColumnName("time_end")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TimeSpawn)
                    .HasColumnName("time_spawn")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Fort)
                    .WithMany(p => p.Raids)
                    .HasForeignKey(d => d.FortId)
                    .HasConstraintName("raids_ibfk_1");
            });

            modelBuilder.Entity<Sightings>(entity =>
            {
                entity.ToTable("sightings");

                entity.HasIndex(e => e.EncounterId)
                    .HasName("ix_sightings_encounter_id");

                entity.HasIndex(e => e.ExpireTimestamp)
                    .HasName("ix_sightings_expire_timestamp");

                entity.HasIndex(e => e.WeatherCellId)
                    .HasName("ix_sightings_weather_cell");

                entity.HasIndex(e => new { e.EncounterId, e.ExpireTimestamp })
                    .HasName("timestamp_encounter_id_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AtkIv).HasColumnName("atk_iv");

                entity.Property(e => e.Cp)
                    .HasColumnName("cp")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.DefIv).HasColumnName("def_iv");

                entity.Property(e => e.EncounterId).HasColumnName("encounter_id");

                entity.Property(e => e.ExpireTimestamp)
                    .HasColumnName("expire_timestamp")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Form)
                    .HasColumnName("form")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Move1)
                    .HasColumnName("move_1")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Move2)
                    .HasColumnName("move_2")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.PokemonId)
                    .HasColumnName("pokemon_id")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.SpawnId)
                    .HasColumnName("spawn_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.StaIv).HasColumnName("sta_iv");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.WeatherBoostedCondition)
                    .HasColumnName("weather_boosted_condition")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.WeatherCellId).HasColumnName("weather_cell_id");

                entity.Property(e => e.Weight)
                    .HasColumnName("weight")
                    .HasColumnType("double(18,14)");

                entity.HasOne(d => d.WeatherCell)
                    .WithMany(p => p.Sightings)
                    .HasPrincipalKey(p => p.S2CellId)
                    .HasForeignKey(d => d.WeatherCellId)
                    .HasConstraintName("sightings_fk_cellid");
            });

            modelBuilder.Entity<Spawnpoints>(entity =>
            {
                entity.ToTable("spawnpoints");

                entity.HasIndex(e => e.DespawnTime)
                    .HasName("ix_spawnpoints_despawn_time");

                entity.HasIndex(e => e.SpawnId)
                    .HasName("ix_spawnpoints_spawn_id")
                    .IsUnique();

                entity.HasIndex(e => e.Updated)
                    .HasName("ix_spawnpoints_updated");

                entity.HasIndex(e => new { e.Lat, e.Lon })
                    .HasName("ix_coords_sp");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DespawnTime)
                    .HasColumnName("despawn_time")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.Failures).HasColumnName("failures");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("double(18,14)");

                entity.Property(e => e.SpawnId)
                    .HasColumnName("spawn_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Weather>(entity =>
            {
                entity.ToTable("weather");

                entity.HasIndex(e => e.S2CellId)
                    .HasName("ix_s2_cell_id")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AlertSeverity)
                    .HasColumnName("alert_severity")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Condition)
                    .HasColumnName("condition")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.Day)
                    .HasColumnName("day")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.S2CellId)
                    .IsRequired()
                    .HasColumnName("s2_cell_id");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Warn)
                    .HasColumnName("warn")
                    .HasColumnType("tinyint(1)");
            });
        }
    }
}
