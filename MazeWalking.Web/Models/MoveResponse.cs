using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Models
{
    public record MoveResponse(
        bool Success,
        string Message,
        PlayersData? PlayerData = null
    );
}
