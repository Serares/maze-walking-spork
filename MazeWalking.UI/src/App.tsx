import { useState, useCallback } from 'react';
import { LandingPage } from './components/LandingPage';
import { MazeGrid } from './components/MazeGrid';
import apiClient from './services/api';
import type { InitResponse } from './types/api';

function App() {
  const [gameState, setGameState] = useState<'landing' | 'playing'>('landing');
  const [mazeData, setMazeData] = useState<InitResponse | null>(null);
  const [playerName, setPlayerName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | undefined>(undefined);

  const handleStartGame = useCallback(async (name: string, gridSize: number) => {
    setIsLoading(true);
    setError(undefined);

    try {
      const response = await apiClient.initMaze({
        playerName: name,
        rowsColumns: gridSize,
      });

      setMazeData(response);
      setPlayerName(name);
      setGameState('playing');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to initialize game';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const handleReset = useCallback(() => {
    setGameState('landing');
    setMazeData(null);
    setPlayerName('');
    setError(undefined);
  }, []);

  return (
    <>
      {gameState === 'landing' ? (
        <LandingPage
          onStart={handleStartGame}
          isLoading={isLoading}
          error={error}
        />
      ) : (
        mazeData && (
          <MazeGrid
            grid={mazeData.grid}
            playerName={playerName}
            onReset={handleReset}
          />
        )
      )}
    </>
  );
}

export default App;
