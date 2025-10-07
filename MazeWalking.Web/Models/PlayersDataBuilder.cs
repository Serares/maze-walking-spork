using MazeWalking.Web.Models.Data;

namespace MazeWalking.Web.Models
{
    /// <summary>
    /// Builder class for constructing PlayersData instances.
    /// </summary>
    public sealed class PlayersDataBuilder
    {
        public PlayersData Pd { get; set; }
        public PlayersDataBuilder GeneratePlayersDataFromInitRequest(InitRequest initRequest)
        {
            PlayersData pd = new()
            {
                Name = initRequest.PlayerName,
                CurrentPosition = new Position(0, 0),
                Maze = new int[initRequest.RowsColumns, initRequest.RowsColumns],
                Finished = false,
            };

            Pd = pd;
            return this;
        }

        public PlayersDataBuilder AddMaze(int[,] maze)
        {
            Pd.Maze = maze;
            return this;
        }
    }
}
