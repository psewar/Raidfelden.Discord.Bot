using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Raidfelden.Data.Pokemon
{
    public partial class raidfeldenContext : DbContext
    {
        public raidfeldenContext()
        {
        }

        public raidfeldenContext(DbContextOptions<raidfeldenContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Trade> Trades { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;port=3306;user=hydro74000;password=hydro74000;database=raidfelden");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.ToTable("trades");

                entity.HasIndex(e => e.UserId)
                    .HasName("UserId");

                entity.Property(e => e.PokemonId).HasColumnType("int(11)");

                entity.Property(e => e.TradeType).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Trades)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_UserId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Active)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.DiscordMention).HasColumnType("varchar(255)");

                entity.Property(e => e.IsTradeAllowed)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Latitude).HasColumnType("double(18,14)");

                entity.Property(e => e.Longitude).HasColumnType("double(18,14)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });
        }
    }
}
