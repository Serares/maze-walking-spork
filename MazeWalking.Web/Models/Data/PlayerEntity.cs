using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MazeWalking.Web.Models.Data
{
    /// <summary>
    /// Entity representing a player in the database.
    /// A player can have multiple matches (one-to-many relationship).
    /// </summary>
    [Table("Players")]
    public class PlayerEntity
    {
        /// <summary>
        /// Primary key for the player.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Player's name. Must be unique.
        /// </summary>
        [Required]
        [Column("name")]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Timestamp when the player was created.
        /// </summary>
        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the player record was last updated.
        /// </summary>
        [Required]
        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property: Collection of matches for this player.
        /// </summary>
        public ICollection<MatchEntity> Matches { get; set; } = new List<MatchEntity>();
    }
}
