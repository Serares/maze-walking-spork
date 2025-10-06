using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Repositories
{
    /// <summary>
    /// Repository interface for managing player game data persistence.
    /// Provides methods for initializing and updating player records in the database.
    /// </summary>
    public interface IGameDataRepository
    {
        /// <summary>
        /// Initializes a new player entry in the database.
        /// Sets CreatedAt and UpdatedAt timestamps automatically.
        /// </summary>
        /// <param name="playersData">The player data to initialize.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The initialized player data with assigned ID.</returns>
        Task<PlayersData> InitializeEntryAsync(PlayersData playersData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing player entry in the database by ID.
        /// Updates the UpdatedAt timestamp automatically.
        /// </summary>
        /// <param name="playersData">The player data to update (must have valid ID).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The updated player data, or null if not found.</returns>
        Task<PlayersData?> UpdateEntryByIdAsync(PlayersData playersData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing player entry in the database by name.
        /// Updates the UpdatedAt timestamp automatically.
        /// </summary>
        /// <param name="playersData">The player data to update (uses Name to find record).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The updated player data, or null if not found.</returns>
        Task<PlayersData?> UpdateEntryByNameAsync(PlayersData playersData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a player entry by ID.
        /// </summary>
        /// <param name="id">The player ID.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The player data if found, otherwise null.</returns>
        Task<PlayersData?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a player entry by name.
        /// </summary>
        /// <param name="name">The player name.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The player data if found, otherwise null.</returns>
        Task<PlayersData?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all player entries from the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A list of all player data records.</returns>
        Task<List<PlayersData>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
