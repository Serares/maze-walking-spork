using Microsoft.EntityFrameworkCore;
using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Data
{
    /// <summary>
    /// Database context for the maze walking game.
    /// Manages Player and Match entities and provides database access through EF Core.
    /// </summary>
    public class GameDataDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the GameDataDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public GameDataDbContext(DbContextOptions<GameDataDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Players entity set.
        /// </summary>
        public DbSet<PlayerEntity> Players { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Matches entity set.
        /// </summary>
        public DbSet<MatchEntity> Matches { get; set; } = null!;

        /// <summary>
        /// Configures the model and entity relationships.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PlayerEntity
            modelBuilder.Entity<PlayerEntity>(entity =>
            {
                entity.ToTable("Players");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .IsRequired();

                // Create unique index on name
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Players_Name");

                // Configure one-to-many relationship with Matches
                entity.HasMany(e => e.Matches)
                    .WithOne(m => m.Player)
                    .HasForeignKey(m => m.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MatchEntity
            modelBuilder.Entity<MatchEntity>(entity =>
            {
                entity.ToTable("Matches");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PlayerId)
                    .HasColumnName("playerId")
                    .IsRequired();

                entity.Property(e => e.CurrentPosition)
                    .HasColumnName("currentPosition")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Maze)
                    .HasColumnName("maze")
                    .IsRequired();

                entity.Property(e => e.Finished)
                    .HasColumnName("finished")
                    .HasDefaultValue(false);

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasDefaultValue(0.0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .IsRequired();

                // Create index on PlayerId for faster lookups
                entity.HasIndex(e => e.PlayerId)
                    .HasDatabaseName("IX_Matches_PlayerId");
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically update timestamps.
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update timestamps.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates CreatedAt and UpdatedAt timestamps for entities being added or modified.
        /// </summary>
        private void UpdateTimestamps()
        {
            var now = DateTime.UtcNow;

            // Update PlayerEntity timestamps
            var playerEntries = ChangeTracker.Entries<PlayerEntity>();
            foreach (var entry in playerEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        // Prevent modification of CreatedAt
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        break;
                }
            }

            // Update MatchEntity timestamps
            var matchEntries = ChangeTracker.Entries<MatchEntity>();
            foreach (var entry in matchEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        // Prevent modification of CreatedAt
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        break;
                }
            }
        }
    }
}
