using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Models
{
    public record InitResponse(PlayersData playersData, List<List<int>> mazeConfig);
}
