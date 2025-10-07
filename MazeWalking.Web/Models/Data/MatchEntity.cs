using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MazeWalking.Web.Models.Data
{
    /// <summary>
    /// Entity representing a single match/game instance in the database.
    /// Belongs to a player (many-to-one relationship).
    /// </summary>
    [Table("Matches")]
    public class MatchEntity
    {
        /// <summary>
        /// Primary key for the match.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the Player table.
        /// </summary>
        [Required]
        [Column("playerId")]
        public Guid PlayerId { get; set; }

        /// <summary>
        /// Current position stored as JSON string in format {"X":1,"Y":10}.
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
        [Column("maze")]
        public required string Maze { get; set; }

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
        /// Timestamp when the match was created.
        /// </summary>
        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the match was last updated.
        /// </summary>
        [Required]
        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property: Reference to the player who owns this match.
        /// </summary>
        public PlayerEntity Player { get; set; } = null!;
    }
}
