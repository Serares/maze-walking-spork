namespace MazeWalking.Web.Models.Data
{
    /// <summary>
    /// Represents a 2D position in the maze grid.
    /// </summary>
    public record Position(int X, int Y);

    /// <summary>
    /// Player data model used for both API request/response and in-memory game state.
    /// This model represents the complete state of a player in the maze walking game.
    /// </summary>
    public class PlayersData
    {
        /// <summary>
        /// Unique identifier for the player (primary key in database).
        /// </summary>
        public int Id { get; set; }

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
        /// This is used for the Initial state of the maze
        /// can be used in case the player wants to restart the same game
        /// </summary>
        public required int[][] Maze { get; set; }

        /// <summary>
        /// Indicates whether the player has completed the maze.
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// Elapsed time in seconds since the player started the maze.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Timestamp when the player record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the player record was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
