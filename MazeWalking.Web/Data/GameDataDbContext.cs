using Microsoft.EntityFrameworkCore;
using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Data
{
    /// <summary>
    /// Database context for the maze walking game.
    /// Manages the PlayerData entity and provides database access through EF Core.
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
        /// Gets or sets the PlayerData entity set.
        /// </summary>
        public DbSet<PlayerDataEntity> PlayerData { get; set; } = null!;

        /// <summary>
        /// Configures the model and entity relationships.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PlayerDataEntity
            modelBuilder.Entity<PlayerDataEntity>(entity =>
            {
                // Table name
                entity.ToTable("PlayerData");

                // Primary key
                entity.HasKey(e => e.Id);

                // Configure properties
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CurrentPosition)
                    .HasColumnName("currentPosition")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Maze)
                    .HasColumnName("maze")
                    .IsRequired();

                entity.Property(e => e.Finished)
                    .HasColumnName("finished")
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .IsRequired()
                    .HasDefaultValue(0.0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .IsRequired();

                // Create index on name for faster lookups
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_PlayerData_Name");
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
            var entries = ChangeTracker.Entries<PlayerDataEntity>();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
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
