using MazeWalking.Web.Models;
using MazeWalking.Web.Models.Data;
using MazeWalking.Web.Repositories;

namespace MazeWalking.Web.Services
{
    /// <summary>
    /// Core game engine service for managing maze walking game logic.
    /// Scoped service that maintains in-memory player state for the duration of a request
    /// and coordinates with the repository for persistence.
    /// </summary>
    public class GameEngine
    {
        private readonly ILogger<GameEngine> _logger;
        private readonly IGameDataRepository _repository;
        private readonly MoveChecker _moveChecker;
        // In-memory cache: player name as key, PlayersData as values
        // This survives for the lifetime of the request (scoped service)
        private readonly Dictionary<string, PlayersData> _playersDataCache;

        public GameEngine(
            ILogger<GameEngine> logger, 
            IGameDataRepository repository,
            MoveChecker moveChecker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _playersDataCache = new Dictionary<string, PlayersData>();
            _moveChecker = moveChecker;

            _logger.LogDebug("GameEngine instance created (scoped)");
        }

        public string InitConfig(InitRequest initRequest)
        {
            // TODO: Implement initialization logic
            _logger.LogInformation("Game configuration initialized");
            return "Config initialized";
        }

        public string Move()
        {
            return "Implement me!";
        }

        public async Task<PlayersData?> GetPlayerAsync(string playerName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException("Player name cannot be null or empty", nameof(playerName));
            }

            if (_playersDataCache.TryGetValue(playerName, out var cachedPlayer))
            {
                _logger.LogDebug("Player {PlayerName} found in cache", playerName);
                return cachedPlayer;
            }

            var player = await _repository.GetByNameAsync(playerName, cancellationToken);

            if (player != null)
            {
                _playersDataCache[playerName] = player;
                _logger.LogDebug("Player {PlayerName} loaded from database and cached", playerName);
            }
            else
            {
                _logger.LogDebug("Player {PlayerName} not found in database", playerName);
            }

            return player;
        }

        public void CachePlayer(PlayersData playersData)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            if (string.IsNullOrWhiteSpace(playersData.Name))
            {
                throw new ArgumentException("Player name cannot be null or empty", nameof(playersData));
            }

            _playersDataCache[playersData.Name] = playersData;
            _logger.LogDebug("Player {PlayerName} cached in memory", playersData.Name);
        }

        public async Task<PlayersData> InitializePlayerAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            _logger.LogInformation("Initializing new player: {PlayerName}", playersData.Name);

            // Initialize in database
            var initializedPlayer = await _repository.InitializeEntryAsync(playersData, cancellationToken);

            // Add to cache
            _playersDataCache[initializedPlayer.Name] = initializedPlayer;

            _logger.LogInformation("Player {PlayerName} initialized with Player ID {PlayerId} and Match ID {MatchId}",
                initializedPlayer.Name, initializedPlayer.PlayerId, initializedPlayer.MatchId);

            return initializedPlayer;
        }
        public async Task<PlayersData?> SavePlayerByIdAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            _logger.LogInformation("Saving player data for Match ID {MatchId}", playersData.MatchId);

            // Update in database
            var updatedPlayer = await _repository.UpdateEntryByIdAsync(playersData, cancellationToken);

            if (updatedPlayer != null)
            {
                // Update cache
                _playersDataCache[updatedPlayer.Name] = updatedPlayer;
                _logger.LogInformation("Match ID {MatchId} saved successfully", updatedPlayer.MatchId);
            }
            else
            {
                _logger.LogWarning("Failed to save Match ID {MatchId} - not found in database", playersData.MatchId);
            }

            return updatedPlayer;
        }

        public async Task<PlayersData?> SavePlayerByNameAsync(PlayersData playersData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playersData);

            _logger.LogInformation("Saving player data for name {PlayerName}", playersData.Name);

            // Update in database
            var updatedPlayer = await _repository.UpdateEntryByNameAsync(playersData, cancellationToken);

            if (updatedPlayer != null)
            {
                // Update cache
                _playersDataCache[updatedPlayer.Name] = updatedPlayer;
                _logger.LogInformation("Player {PlayerName} saved successfully", updatedPlayer.Name);
            }
            else
            {
                _logger.LogWarning("Failed to save player {PlayerName} - not found in database", playersData.Name);
            }

            return updatedPlayer;
        }

        public async Task<List<PlayersData>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving all players from database");
            return await _repository.GetAllAsync(cancellationToken);
        }

        public IReadOnlyDictionary<string, PlayersData> GetCachedPlayers()
        {
            _logger.LogDebug("Returning {Count} cached players", _playersDataCache.Count);
            return _playersDataCache;
        }
    }
}
