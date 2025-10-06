import { useState, type FormEvent } from 'react';

interface LandingPageProps {
  onStart: (playerName: string, gridSize: number) => void;
  isLoading: boolean;
  error?: string;
}

export function LandingPage({ onStart, isLoading, error }: LandingPageProps) {
  const [playerName, setPlayerName] = useState('');
  const [gridSize, setGridSize] = useState('10');

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const size = parseInt(gridSize, 10);
    if (!playerName.trim()) {
      alert('Please enter a player name');
      return;
    }
    if (isNaN(size) || size < 3 || size > 50) {
      alert('Please enter a grid size between 3 and 50');
      return;
    }

    onStart(playerName.trim(), size);
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-br from-emerald-50 to-emerald-100 p-5">
      <div className="bg-white rounded-2xl p-10 shadow-xl max-w-md w-full border-2 border-emerald-100">
        <h1 className="text-center text-emerald-800 mb-3 text-3xl font-bold">
          Maze Walking Game
        </h1>

        <p className="text-center text-slate-600 mb-8 text-sm">
          Enter your name and choose a maze size to begin
        </p>

        <form onSubmit={handleSubmit}>
          <div className="mb-5">
            <label
              htmlFor="playerName"
              className="block mb-2 text-slate-700 text-sm font-medium"
            >
              Player Name
            </label>
            <input
              id="playerName"
              type="text"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="Enter your name"
              disabled={isLoading}
              className="w-full px-4 py-3 text-base border-2 border-emerald-200 rounded-lg
                         focus:outline-none focus:border-emerald-700 focus:ring-2 focus:ring-emerald-200
                         disabled:bg-slate-50 disabled:cursor-not-allowed
                         text-slate-800 placeholder-slate-400 transition-colors"
            />
          </div>

          <div className="mb-8">
            <label
              htmlFor="gridSize"
              className="block mb-2 text-slate-700 text-sm font-medium"
            >
              Grid Size (rows x columns)
            </label>
            <input
              id="gridSize"
              type="number"
              value={gridSize}
              onChange={(e) => setGridSize(e.target.value)}
              placeholder="10"
              min="3"
              max="50"
              disabled={isLoading}
              className="w-full px-4 py-3 text-base border-2 border-emerald-200 rounded-lg
                         focus:outline-none focus:border-emerald-700 focus:ring-2 focus:ring-emerald-200
                         disabled:bg-slate-50 disabled:cursor-not-allowed
                         text-slate-800 placeholder-slate-400 transition-colors"
            />
            <p className="mt-2 text-xs text-slate-500">
              Choose a number between 3 and 50
            </p>
          </div>

          {error && (
            <div className="px-4 py-3 mb-5 bg-red-100 text-red-700 rounded-lg text-sm border border-red-200">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-4 text-base font-semibold text-white
                       bg-emerald-800 hover:bg-emerald-700
                       disabled:bg-slate-400 disabled:cursor-not-allowed
                       rounded-lg transition-colors duration-200
                       focus:outline-none focus:ring-4 focus:ring-emerald-300"
          >
            {isLoading ? 'Starting Game...' : 'Start Game'}
          </button>
        </form>
      </div>
    </div>
  );
}
