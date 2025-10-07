using Microsoft.EntityFrameworkCore;
using MazeWalking.Web.Data;
using MazeWalking.Web.Models.Data;
using System.Text.Json;

namespace MazeWalking.Web.Repositories
{
    /// <summary>
    /// Repository implementation for managing player and match data using EF Core and SQLite.
    /// Works with normalized Player and Match tables.
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
                // Find or create player
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Name == playersData.Name, cancellationToken);

                if (player == null)
                {
                    player = new PlayerEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = playersData.Name
                    };
                    await _context.Players.AddAsync(player, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Created new player {PlayerName} with ID {PlayerId}",
                        player.Name, player.Id);
                }

                // Create new match for this player
                var positionJson = JsonSerializer.Serialize(playersData.CurrentPosition);
                var mazeJson = JsonSerializer.Serialize(playersData.Maze);

                var match = new MatchEntity
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    CurrentPosition = positionJson,
                    Maze = mazeJson,
                    Finished = playersData.Finished,
                    Time = playersData.Time
                };

                await _context.Matches.AddAsync(match, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Initialized match {MatchId} for player {PlayerName}",
                    match.Id, player.Name);

                return PlayersData.FromEntities(player, match);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while initializing entry for {PlayerName}",
                    playersData.Name);
                throw new InvalidOperationException($"Failed to initialize entry for {playersData.Name}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while initializing entry for {PlayerName}",
                    playersData.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> UpdateEntryByIdAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            if (String.IsNullOrEmpty(playersData.MatchId.ToString()))
            {
                throw new ArgumentException("Match ID must be greater than zero", nameof(playersData));
            }

            try
            {
                // Find existing match with player
                var match = await _context.Matches
                    .Include(m => m.Player)
                    .FirstOrDefaultAsync(m => m.Id == playersData.MatchId, cancellationToken);

                if (match == null)
                {
                    _logger.LogWarning("Match with ID {MatchId} not found for update", playersData.MatchId);
                    return null;
                }

                // Update match properties
                var positionJson = JsonSerializer.Serialize(playersData.CurrentPosition);
                var mazeJson = JsonSerializer.Serialize(playersData.Maze);

                match.CurrentPosition = positionJson;
                match.Maze = mazeJson;
                match.Finished = playersData.Finished;
                match.Time = playersData.Time;

                _context.Matches.Update(match);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated match ID {MatchId}", playersData.MatchId);

                return PlayersData.FromEntities(match.Player, match);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating match with ID {MatchId}",
                    playersData.MatchId);
                throw new InvalidOperationException($"Concurrency conflict while updating match {playersData.MatchId}", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating match with ID {MatchId}",
                    playersData.MatchId);
                throw new InvalidOperationException($"Failed to update match with ID {playersData.MatchId}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating match with ID {MatchId}",
                    playersData.MatchId);
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

            if (String.IsNullOrEmpty(playersData.MatchId.ToString()))
            {
                throw new ArgumentException("Match ID must be greater than zero", nameof(playersData));
            }

            try
            {
                // Find player by name
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Name == playersData.Name, cancellationToken);

                if (player == null)
                {
                    _logger.LogWarning("Player with name '{PlayerName}' not found for update", playersData.Name);
                    return null;
                }

                // Find the specific match
                var match = await _context.Matches
                    .FirstOrDefaultAsync(m => m.Id == playersData.MatchId && m.PlayerId == player.Id, cancellationToken);

                if (match == null)
                {
                    _logger.LogWarning("Match {MatchId} not found for player '{PlayerName}'",
                        playersData.MatchId, playersData.Name);
                    return null;
                }

                // Update match properties
                var positionJson = JsonSerializer.Serialize(playersData.CurrentPosition);
                var mazeJson = JsonSerializer.Serialize(playersData.Maze);

                match.CurrentPosition = positionJson;
                match.Maze = mazeJson;
                match.Finished = playersData.Finished;
                match.Time = playersData.Time;

                _context.Matches.Update(match);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated match {MatchId} for player '{PlayerName}'",
                    match.Id, playersData.Name);

                return PlayersData.FromEntities(player, match);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating match for '{PlayerName}'",
                    playersData.Name);
                throw new InvalidOperationException($"Concurrency conflict while updating match for '{playersData.Name}'", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating match for '{PlayerName}'",
                    playersData.Name);
                throw new InvalidOperationException($"Failed to update match for '{playersData.Name}'", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating match for '{PlayerName}'",
                    playersData.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PlayersData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get match with player by match ID
                var match = await _context.Matches
                    .AsNoTracking()
                    .Include(m => m.Player)
                    .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

                if (match == null)
                {
                    _logger.LogDebug("Match with ID {MatchId} not found", id);
                    return null;
                }

                return PlayersData.FromEntities(match.Player, match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving match with ID {MatchId}", id);
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
                // Get player by name with their most recent match
                var player = await _context.Players
                    .AsNoTracking()
                    .Include(p => p.Matches.OrderByDescending(m => m.UpdatedAt).Take(1))
                    .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

                if (player == null)
                {
                    _logger.LogDebug("Player with name '{PlayerName}' not found", name);
                    return null;
                }

                var latestMatch = player.Matches.FirstOrDefault();
                if (latestMatch == null)
                {
                    _logger.LogDebug("Player '{PlayerName}' has no matches", name);
                    return null;
                }

                return PlayersData.FromEntities(player, latestMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving player with name '{PlayerName}'", name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PlayersData>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all players with their latest match
                var players = await _context.Players
                    .AsNoTracking()
                    .Include(p => p.Matches.OrderByDescending(m => m.UpdatedAt).Take(1))
                    .ToListAsync(cancellationToken);

                var result = new List<PlayersData>();

                foreach (var player in players)
                {
                    var latestMatch = player.Matches.FirstOrDefault();
                    if (latestMatch != null)
                    {
                        result.Add(PlayersData.FromEntities(player, latestMatch));
                    }
                }

                _logger.LogInformation("Retrieved {Count} player entries with matches", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all player entries");
                throw;
            }
        }
    }
}
