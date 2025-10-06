using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MazeWalking.Web.Models.Data
{
    /// <summary>
    /// Entity model for storing player data in the database using EF Core.
    /// This entity uses JSON serialization for complex types (Position and Maze).
    /// </summary>
    [Table("PlayerData")]
    public class PlayerDataEntity
    {
        /// <summary>
        /// Primary key for the player data record.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Player's name stored as a string.
        /// </summary>
        [Required]
        [Column("name")]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Current position stored as JSON string in format {"x": 1, "y": 10}.
        /// Example: {"x": 5, "y": 10}
        /// </summary>
        [Required]
        [Column("currentPosition")]
        [MaxLength(100)]
        public required string CurrentPosition { get; set; }

        /// <summary>
        /// Maze structure stored as JSON string representing a 2D array.
        /// Example: "[[0,1,0],[0,0,1],[1,0,0]]"
        /// </summary>
        [Required]
        [Column("initialmaze")]
        public required string InitialMaze { get; set; }

        [Required]
        [Column("currentmaze")]
        public required string CurrentMaze { get; set; }

        /// <summary>
        /// Indicates whether the player has finished the maze.
        /// </summary>
        [Column("finished")]
        public bool Finished { get; set; }

        /// <summary>
        /// Elapsed time in seconds.
        /// </summary>
        [Column("time")]
        public double Time { get; set; }

        /// <summary>
        /// Timestamp when the record was created.
        /// Stored as ISO 8601 string format.
        /// </summary>
        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the record was last updated.
        /// Stored as ISO 8601 string format.
        /// </summary>
        [Required]
        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Converts this entity to a PlayersData model.
        /// </summary>
        /// <returns>A PlayersData instance with deserialized complex types.</returns>
        public PlayersData ToPlayersData()
        {
            // Use case-insensitive deserialization to support both {"X":1,"Y":10} and {"x":1,"y":10}
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize position from JSON format
            var position = JsonSerializer.Deserialize<Position>(CurrentPosition, jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize position data");

            // Deserialize maze from JSON
            var maze = JsonSerializer.Deserialize<int[][]>(Maze, jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize maze data");

            return new PlayersData
            {
                Id = Id,
                Name = Name,
                CurrentPosition = position,
                Maze = maze,
                Finished = Finished,
                Time = Time,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }

        /// <summary>
        /// Creates a PlayerDataEntity from a PlayersData model.
        /// </summary>
        /// <param name="playersData">The source PlayersData model.</param>
        /// <returns>A new PlayerDataEntity with serialized complex types.</returns>
        public static PlayerDataEntity FromPlayersData(PlayersData playersData)
        {
            // Serialize position to JSON format: {"x": 1, "y": 10}
            var positionJson = JsonSerializer.Serialize(playersData.CurrentPosition);

            // Serialize maze to JSON
            var mazeJson = JsonSerializer.Serialize(playersData.Maze);

            return new PlayerDataEntity
            {
                Id = playersData.Id,
                Name = playersData.Name,
                CurrentPosition = positionJson,
                Maze = mazeJson,
                Finished = playersData.Finished,
                Time = playersData.Time,
                CreatedAt = playersData.CreatedAt,
                UpdatedAt = playersData.UpdatedAt
            };
        }
    }
}
