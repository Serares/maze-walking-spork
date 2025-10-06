using Microsoft.EntityFrameworkCore;
using MazeWalking.Web.Data;
using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Repositories
{
    /// <summary>
    /// Repository implementation for managing player game data using EF Core and SQLite.
    /// Handles conversion between PlayersData models and PlayerDataEntity entities.
    /// </summary>
    public class GameDataRepository : IGameDataRepository
    {
        private readonly GameDataDbContext _context;
        private readonly ILogger<GameDataRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the GameDataRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">Logger for diagnostics and error tracking.</param>
        public GameDataRepository(GameDataDbContext context, ILogger<GameDataRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PlayersData> InitializeEntryAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            try
            {
                // Convert to entity
                var entity = PlayerDataEntity.FromPlayersData(playersData);

                // Add to context

                await _context.PlayerData.AddAsync(entity, cancellationToken);
                // Save changes (timestamps are set automatically by the context)
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Initialized player entry for {PlayerName} with ID {PlayerId}",
                    entity.Name, entity.Id);

                // Convert back to model and return
                return entity.ToPlayersData();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while initializing player entry for {PlayerName}",
                    playersData.Name);
                throw new InvalidOperationException($"Failed to initialize player entry for {playersData.Name}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while initializing player entry for {PlayerName}",
                    playersData.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> UpdateEntryByIdAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            if (playersData.Id <= 0)
            {
                throw new ArgumentException("Player ID must be greater than zero", nameof(playersData));
            }

            try
            {
                // Find existing entity
                var existingEntity = await _context.PlayerData
                    .FirstOrDefaultAsync(p => p.Id == playersData.Id, cancellationToken);

                if (existingEntity == null)
                {
                    _logger.LogWarning("Player entry with ID {PlayerId} not found for update", playersData.Id);
                    return null;
                }

                // Update entity properties (preserve ID and CreatedAt)
                var updatedEntity = PlayerDataEntity.FromPlayersData(playersData);
                existingEntity.Name = updatedEntity.Name;
                existingEntity.CurrentPosition = updatedEntity.CurrentPosition;
                existingEntity.InitialMaze = updatedEntity.InitialMaze;
                existingEntity.Finished = updatedEntity.Finished;
                existingEntity.Time = updatedEntity.Time;
                // UpdatedAt is handled by the context automatically

                // Mark as modified
                _context.PlayerData.Update(existingEntity);

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated player entry for ID {PlayerId}", playersData.Id);

                // Convert back to model and return
                return existingEntity.ToPlayersData();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating player entry with ID {PlayerId}",
                    playersData.Id);
                throw new InvalidOperationException($"Concurrency conflict while updating player with ID {playersData.Id}", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating player entry with ID {PlayerId}",
                    playersData.Id);
                throw new InvalidOperationException($"Failed to update player entry with ID {playersData.Id}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating player entry with ID {PlayerId}",
                    playersData.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> UpdateEntryByNameAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            if (string.IsNullOrWhiteSpace(playersData.Name))
            {
                throw new ArgumentException("Player name cannot be null or empty", nameof(playersData));
            }

            try
            {
                // Find existing entity by name
                var existingEntity = await _context.PlayerData
                    .FirstOrDefaultAsync(p => p.Name == playersData.Name, cancellationToken);

                if (existingEntity == null)
                {
                    _logger.LogWarning("Player entry with name '{PlayerName}' not found for update", playersData.Name);
                    return null;
                }

                // Update entity properties (preserve ID and CreatedAt)
                var updatedEntity = PlayerDataEntity.FromPlayersData(playersData);
                existingEntity.CurrentPosition = updatedEntity.CurrentPosition;
                existingEntity.Maze = updatedEntity.Maze;
                existingEntity.Finished = updatedEntity.Finished;
                existingEntity.Time = updatedEntity.Time;
                // UpdatedAt is handled by the context automatically

                // Mark as modified
                _context.PlayerData.Update(existingEntity);

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated player entry for name '{PlayerName}'", playersData.Name);

                // Convert back to model and return
                return existingEntity.ToPlayersData();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating player entry with name '{PlayerName}'",
                    playersData.Name);
                throw new InvalidOperationException($"Concurrency conflict while updating player '{playersData.Name}'", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating player entry with name '{PlayerName}'",
                    playersData.Name);
                throw new InvalidOperationException($"Failed to update player entry '{playersData.Name}'", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating player entry with name '{PlayerName}'",
                    playersData.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Player ID must be greater than zero", nameof(id));
            }

            try
            {
                var entity = await _context.PlayerData
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                if (entity == null)
                {
                    _logger.LogDebug("Player entry with ID {PlayerId} not found", id);
                    return null;
                }

                return entity.ToPlayersData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving player entry with ID {PlayerId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Player name cannot be null or empty", nameof(name));
            }

            try
            {
                var entity = await _context.PlayerData
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

                if (entity == null)
                {
                    _logger.LogDebug("Player entry with name '{PlayerName}' not found", name);
                    return null;
                }

                return entity.ToPlayersData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving player entry with name '{PlayerName}'", name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PlayersData>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.PlayerData
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} player entries", entities.Count);

                return entities.Select(e => e.ToPlayersData()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all player entries");
                throw;
            }
        }
    }
}
