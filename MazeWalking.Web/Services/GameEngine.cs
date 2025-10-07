using System.Security.Cryptography;
using MazeWalking.Web.Models;
using MazeWalking.Web.Models.Data;
using MazeWalking.Web.Repositories;
using MazeWalking.Web.Utils;

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
        private PlayersData _playersDataCache { get; set; }
        // this number should be adjusted based on the
        // InitRequest.RowsCount
        private int _Obstacles { get; set; } = 0;
        private int _ObstaclesPlaced { get; set; } = 0;
        private double Divider { get; } = 0.33;

        public GameEngine(
            ILogger<GameEngine> logger,
            IGameDataRepository repository,
            MoveChecker moveChecker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _moveChecker = moveChecker;

            _logger.LogDebug("GameEngine instance created (scoped)");
        }

        public async Task<PlayersData> InitConfig(InitRequest initRequest)
        {
            AdjustObstacles(initRequest);
            var pd = new PlayersDataBuilder().GeneratePlayersDataFromInitRequest(initRequest);
            pd.AddMaze(GenerateMaze(initRequest.RowsColumns, initRequest.RowsColumns));
            _playersDataCache = pd.Pd;

            PlayersData _pd = await _repository.InitializeEntryAsync(_playersDataCache);
            _logger.LogInformation("Game configuration initialized");
            return _pd;
        }

        private int[,] GenerateMaze(int rows, int cols)
        {

            if (cols == 0 || rows == 0) throw new ArgumentOutOfRangeException();
            int[,] maze2d = new int[rows, cols];

            for (int i = 0; i < maze2d.GetLength(0); i++)
            {
                for (int j = 0; j < maze2d.GetLength(1); j++)
                {
                    maze2d[i, j] = PathGenerator(i, j, i == maze2d.GetLength(0) - 1 && j == maze2d.GetLength(1) - 1);
                }
            }

            if (_ObstaclesPlaced < _Obstacles) CompleteMazeObstacles(maze2d);

            return maze2d;
        }
        // should generate the right
        // value for the cells
        private int PathGenerator(int x, int y, bool isLast)
        {
            if (isLast) return 2;
            if (x == 0 && y == 0)
            {
                return (int)ECellType.Player;
            }

            // try adding obstacles
            if (_ObstaclesPlaced < _Obstacles && x != 0 && y != 0)
            {
                double remainingRation = (double)(_Obstacles - _ObstaclesPlaced) / _Obstacles;
                double probability = 0.3 * 0.5 * remainingRation;

                double roll = RandomUtil.RandomDouble();
                if (roll < probability)
                {
                    _ObstaclesPlaced++;
                    return (int)ECellType.Obstacle;
                }
            }

            return (int)ECellType.Path;
        }

        private void CompleteMazeObstacles(int[,] maze2d)
        {
            while (_ObstaclesPlaced < _Obstacles)
            {
                int ri = RandomNumberGenerator.GetInt32(1, maze2d.GetLength(0) - 1);
                int rj = RandomNumberGenerator.GetInt32(1, maze2d.GetLength(1) - 1);

                if (maze2d[ri, rj] == (int)ECellType.Path)
                {
                    maze2d[ri, rj] = (int)ECellType.Obstacle;
                    _ObstaclesPlaced++;
                }
            }
        }

        private void AdjustObstacles(InitRequest initRequest)
        {
            double result = initRequest.RowsColumns * initRequest.RowsColumns / Divider;
            _Obstacles = (int)Math.Ceiling(result);
        }

        public string Move()
        {
            return "Implement me!";
        }

    }
}
