import { useState, useEffect, useCallback } from 'react';
import apiClient from '../services/api';
import type { PlayerData } from '../types/api';

interface MazeGridProps {
  grid: number[][];
  playerData: PlayerData;
  onReset: () => void;
}

export function MazeGrid({ grid, playerData, onReset }: MazeGridProps) {
  const [playerPosition, setPlayerPosition] = useState({ x: 0, y: 0 });
  const [error, setError] = useState<string | null>(null);
  const [isMoving, setIsMoving] = useState(false);

  // Handle arrow key presses
  const handleKeyPress = useCallback(async (event: KeyboardEvent) => {
    // Prevent default behavior for arrow keys (scrolling)
    if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(event.key)) {
      event.preventDefault();
    }

    if (isMoving) return; // Prevent multiple simultaneous moves

    let newX = playerPosition.x;
    let newY = playerPosition.y;

    switch (event.key) {
      case 'ArrowUp':
        newY = playerPosition.y - 1;
        break;
      case 'ArrowDown':
        newY = playerPosition.y + 1;
        break;
      case 'ArrowLeft':
        newX = playerPosition.x - 1;
        break;
      case 'ArrowRight':
        newX = playerPosition.x + 1;
        break;
      default:
        return; // Ignore other keys
    }

    // Check if move is within bounds
    if (newX < 0 || newX >= grid[0].length || newY < 0 || newY >= grid.length) {
      setError('Cannot move outside the maze!');
      setTimeout(() => setError(null), 2000);
      return;
    }

    // Send move request to backend
    setIsMoving(true);
    setError(null);

    try {
      const response = await apiClient.movePlayer({
        playerId: playerData.playerId,
        x: newX,
        y: newY
      });

      if (response.success) {
        setPlayerPosition({ x: newX, y: newY });
      } else {
        setError(response.message || 'Move not allowed');
        setTimeout(() => setError(null), 2000);
      }
    } catch {
      setError('Failed to move. Please try again.');
      setTimeout(() => setError(null), 2000);
    } finally {
      setIsMoving(false);
    }
  }, [playerPosition, grid, isMoving, playerData.playerId]);

  // Add event listener for keyboard input
  useEffect(() => {
    window.addEventListener('keydown', handleKeyPress);
    return () => {
      window.removeEventListener('keydown', handleKeyPress);
    };
  }, [handleKeyPress]);

  // Calculate cell size based on grid dimensions
  const cellSize = Math.min(40, Math.floor(600 / Math.max(grid.length, grid[0]?.length || 1)));

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-br from-emerald-50 to-slate-100 p-5">
      {/* Header */}
      <div className="mb-5 text-center">
        <h1 className="text-emerald-800 mb-2 text-3xl font-bold">
          Maze Walking Game
        </h1>
        <p className="text-slate-700 text-base mb-1">
          Player: <strong className="text-emerald-700">{playerData.name}</strong>
        </p>
        <p className="text-slate-600 text-sm">
          Use arrow keys to move
        </p>
      </div>

      {/* Error message */}
      {error && (
        <div className="px-5 py-3 mb-4 bg-red-100 text-red-700 rounded-lg text-sm font-medium border border-red-200 shadow-sm">
          {error}
        </div>
      )}

      {/* Maze grid */}
      <div className="bg-white rounded-xl p-5 shadow-xl mb-5 border-2 border-emerald-100">
        <div className="inline-block">
          {grid.map((row, rowIndex) => (
            <div key={rowIndex} className="flex">
              {row.map((cell, colIndex) => {
                const isPlayer = playerPosition.x === colIndex && playerPosition.y === rowIndex;
                const isGoal = rowIndex === grid.length - 1 && colIndex === grid[0].length - 1;

                // Determine cell styling based on cell type
                let cellBgClass = '';
                let cellContent = '';

                if (isPlayer) {
                  // Player cell - highest priority
                  cellBgClass = 'bg-emerald-700 text-white';
                  cellContent = '★';
                } else if (isGoal) {
                  // Goal cell - bottom-right corner
                  cellBgClass = 'bg-emerald-500 text-white';
                  cellContent = '🏁';
                } else if (cell === 0) {
                  // Walkable path
                  cellBgClass = 'bg-emerald-50';
                } else if (cell === 1) {
                  // Wall/Obstacle
                  cellBgClass = 'bg-slate-800';
                }

                return (
                  <div
                    key={`${rowIndex}-${colIndex}`}
                    className={`
                      flex items-center justify-center
                      border border-emerald-200
                      transition-colors duration-200
                      ${cellBgClass}
                    `}
                    style={{
                      width: `${cellSize}px`,
                      height: `${cellSize}px`,
                      fontSize: `${cellSize * 0.6}px`
                    }}
                  >
                    {cellContent}
                  </div>
                );
              })}
            </div>
          ))}
        </div>
      </div>

      {/* Position info */}
      <div className="mb-4 text-slate-700 text-sm font-medium">
        Position: <span className="text-emerald-700">({playerPosition.x}, {playerPosition.y})</span>
      </div>

      {/* Reset button */}
      <button
        onClick={onReset}
        className="px-6 py-3 text-base font-semibold text-white
                   bg-emerald-800 hover:bg-emerald-700
                   rounded-lg transition-colors duration-200
                   focus:outline-none focus:ring-4 focus:ring-emerald-300
                   shadow-md hover:shadow-lg"
      >
        New Game
      </button>

      {/* Instructions */}
      <div className="mt-8 p-5 bg-white rounded-xl max-w-lg shadow-lg border border-emerald-100">
        <h3 className="text-emerald-800 mt-0 mb-3 text-lg font-bold">
          How to Play
        </h3>
        <ul className="text-slate-700 text-sm leading-relaxed pl-5 space-y-2">
          <li>Use arrow keys (↑ ↓ ← →) to move your player (★)</li>
          <li><span className="inline-block w-4 h-4 bg-emerald-50 border border-emerald-200 align-middle mr-1"></span> Light cells are walkable paths</li>
          <li><span className="inline-block w-4 h-4 bg-slate-800 align-middle mr-1"></span> Dark cells are walls (obstacles)</li>
          <li><span className="inline-block w-4 h-4 bg-emerald-500 text-white text-xs leading-4 text-center align-middle mr-1">🏁</span> Reach the goal at the bottom-right corner!</li>
          <li>Navigate through the maze and find your way to the goal</li>
        </ul>
      </div>
    </div>
  );
}
