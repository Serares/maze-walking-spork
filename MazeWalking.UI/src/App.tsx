import { useState, useCallback, useEffect } from 'react';
import { LandingPage } from './components/LandingPage';
import { MazeGrid } from './components/MazeGrid';
import apiClient from './services/api';
import type { InitResponse } from './types/api';
import { loadSession, saveSession, clearSession } from './utils/sessionStorage';

function App() {
  const [gameState, setGameState] = useState<'landing' | 'playing'>('landing');
  const [mazeData, setMazeData] = useState<InitResponse | null>(null);
  const [playerName, setPlayerName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | undefined>(undefined);

  // Check for existing session on mount
  useEffect(() => {
    const existingSession = loadSession();
    if (existingSession) {
      // Auto-resume existing session
      handleStartGame(existingSession.playerName, existingSession.grid.length, existingSession.playerId);
    }
  }, []);

  const handleStartGame = useCallback(async (name: string, gridSize: number, existingPlayerId?: string) => {
    setIsLoading(true);
    setError(undefined);

    try {
      const response = await apiClient.initMaze({
        playerName: name,
        rowsColumns: gridSize,
        playerId: existingPlayerId, // Send existing playerId if resuming
      });

      // Save session to localStorage
      saveSession(response);

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
    // Clear localStorage session
    clearSession();

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
            playerData={mazeData.playerData}
            onReset={handleReset}
          />
        )
      )}
    </>
  );
}

export default App;
