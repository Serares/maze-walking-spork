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

        /// <summary>
        /// Processes a player's move request with full validation and timer management.
        /// </summary>
        /// <param name="moveRequest">The move request containing PlayerId (MatchId), target X and Y coordinates.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>MoveResponse with success status, message, and updated player data.</returns>
        public async Task<MoveResponse> Move(MoveRequest moveRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Guid.TryParse(moveRequest.PlayerId, out Guid matchId))
                {
                    _logger.LogWarning("Invalid PlayerId format: {PlayerId}", moveRequest.PlayerId);
                    return new MoveResponse(false, "Invalid player ID format");
                }

                PlayersData? match = await _repository.GetByIdAsync(matchId, cancellationToken);
                if (match == null)
                {
                    _logger.LogWarning("Match not found: {MatchId}", matchId);
                    return new MoveResponse(false, "Match not found");
                }

                if (match.Finished)
                {
                    _logger.LogInformation("Attempted move on finished game: {MatchId}", matchId);
                    return new MoveResponse(false, "Game is already finished", match);
                }

                int targetX = moveRequest.X;
                int targetY = moveRequest.Y;
                int mazeRows = match.Maze.GetLength(0);
                int mazeCols = match.Maze.GetLength(1);

                if (targetX < 0 || targetX >= mazeRows || targetY < 0 || targetY >= mazeCols)
                {
                    _logger.LogWarning("Move out of bounds: ({X},{Y}) for maze size {Rows}x{Cols}",
                        targetX, targetY, mazeRows, mazeCols);
                    return new MoveResponse(false, "Move is out of bounds", match);
                }

                int cellValue = match.Maze[targetX, targetY];
                if (cellValue == (int)ECellType.Obstacle)
                {
                    _logger.LogWarning("Move to obstacle cell at ({X},{Y})", targetX, targetY);
                    return new MoveResponse(false, "Cannot move to obstacle", match);
                }

                // Validate move is exactly one step (no diagonal, no jumps)
                int currentX = match.CurrentPosition.X;
                int currentY = match.CurrentPosition.Y;
                int deltaX = Math.Abs(targetX - currentX);
                int deltaY = Math.Abs(targetY - currentY);

                // Valid moves: exactly one step horizontally OR vertically (not both, not zero)
                bool isValidStep = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);

                if (!isValidStep)
                {
                    _logger.LogWarning("Invalid move from ({CurrentX},{CurrentY}) to ({TargetX},{TargetY})",
                        currentX, currentY, targetX, targetY);
                    return new MoveResponse(false, "Move must be exactly one step horizontally or vertically", match);
                }

                match.CurrentPosition = new Position(targetX, targetY);

                // Calculate elapsed time from match creation
                match.Time = (DateTime.UtcNow - match.CreatedAt).TotalSeconds;

                // Check if player reached goal (last cell in maze)
                bool reachedGoal = (targetX == mazeRows - 1 && targetY == mazeCols - 1);
                if (reachedGoal)
                {
                    match.Finished = true;
                    _logger.LogInformation("Player reached goal! MatchId: {MatchId}, Time: {Time}s",
                        matchId, match.Time);
                }

                // Persist changes
                PlayersData? updatedMatch = await _repository.UpdateEntryByIdAsync(match, cancellationToken);
                if (updatedMatch == null)
                {
                    _logger.LogError("Failed to update match: {MatchId}", matchId);
                    return new MoveResponse(false, "Failed to update game state");
                }

                string message = reachedGoal
                    ? $"Congratulations! You finished in {updatedMatch.Time:F2} seconds!"
                    : "Move successful";

                _logger.LogDebug("Move successful: MatchId {MatchId}, Position ({X},{Y}), Time {Time}s",
                    matchId, targetX, targetY, updatedMatch.Time);

                return new MoveResponse(true, message, updatedMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing move for PlayerId: {PlayerId}", moveRequest.PlayerId);
                return new MoveResponse(false, "An error occurred while processing the move");
            }
        }

    }
}
