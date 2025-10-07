using System.Text.Json;
using System.Text.Json.Serialization;

namespace MazeWalking.Web.Models.Data
{
    /// <summary>
    /// Represents a 2D position in the maze grid.
    /// </summary>
    public record Position(int X, int Y);

    /// <summary>
    /// Player data model used for both API request/response and in-memory game state.
    /// This model represents the complete state of a player in a current match.
    /// </summary>
    public class PlayersData
    {
        /// <summary>
        /// Unique identifier for the player.
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// Unique identifier for the current match.
        /// </summary>
        public Guid MatchId { get; set; }

        /// <summary>
        /// Player's name. Used as a friendly identifier.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Current position of the player in the maze.
        /// </summary>
        public required Position CurrentPosition { get; set; }

        /// <summary>
        /// The maze structure as a 2D array.
        /// Each cell can represent walls (1), paths (0), start point, end point, etc.
        /// </summary>
        public required int[,] Maze { get; set; }

        /// <summary>
        /// Indicates whether the player has completed the maze.
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// Elapsed time in seconds since the player started the maze.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Timestamp when the match was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the match was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Converts a PlayerEntity and MatchEntity to a PlayersData model.
        /// </summary>
        public static PlayersData FromEntities(PlayerEntity player, MatchEntity match)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var position = JsonSerializer.Deserialize<Position>(match.CurrentPosition, jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize position data");

            var maze = JsonSerializer.Deserialize<int[,]>(match.Maze, jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize maze data");

            return new PlayersData
            {
                PlayerId = player.Id,
                MatchId = match.Id,
                Name = player.Name,
                CurrentPosition = position,
                Maze = maze,
                Finished = match.Finished,
                Time = match.Time,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            };
        }

        /// <summary>
        /// Converts this PlayersData to a MatchEntity (for updating match data).
        /// </summary>
        public MatchEntity ToMatchEntity()
        {
            var positionJson = JsonSerializer.Serialize(CurrentPosition);
            var mazeJson = JsonSerializer.Serialize(Maze);

            return new MatchEntity
            {
                Id = MatchId,
                PlayerId = PlayerId,
                CurrentPosition = positionJson,
                Maze = mazeJson,
                Finished = Finished,
                Time = Time,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }
}
