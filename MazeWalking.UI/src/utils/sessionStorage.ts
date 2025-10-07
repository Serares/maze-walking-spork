import type { InitResponse } from '../types/api';

const SESSION_KEY = 'mazeSession';
const SESSION_EXPIRY = 24 * 60 * 60 * 1000; // 24 hours in milliseconds

export interface MazeSession {
  playerId: string;
  playerName: string;
  grid: number[][];
  lastPlayed: number;
}

/**
 * Save the current game session to localStorage
 */
export function saveSession(response: InitResponse): void {
  try {
    const session: MazeSession = {
      playerId: response.playerData.playerId,
      playerName: response.playerData.name,
      grid: response.grid,
      lastPlayed: Date.now(),
    };

    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  } catch (error) {
    console.error('Failed to save session to localStorage:', error);
  }
}

/**
 * Load the game session from localStorage
 * Returns null if no session exists or if it has expired
 */
export function loadSession(): MazeSession | null {
  try {
    const stored = localStorage.getItem(SESSION_KEY);
    if (!stored) {
      return null;
    }

    const session: MazeSession = JSON.parse(stored);

    // Check if session has expired
    if (Date.now() - session.lastPlayed > SESSION_EXPIRY) {
      clearSession();
      return null;
    }

    return session;
  } catch (error) {
    console.error('Failed to load session from localStorage:', error);
    return null;
  }
}

/**
 * Clear the current game session from localStorage
 */
export function clearSession(): void {
  try {
    localStorage.removeItem(SESSION_KEY);
  } catch (error) {
    console.error('Failed to clear session from localStorage:', error);
  }
}

/**
 * Check if a valid session exists
 */
export function hasValidSession(): boolean {
  const session = loadSession();
  return session !== null;
}
