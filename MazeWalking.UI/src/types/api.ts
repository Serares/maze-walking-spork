// API response types based on MazeWalking.Web endpoints

// Base API error type
export interface ApiError {
  message: string;
  status: number;
  details?: unknown;
}

// Generic API response wrapper
export interface ApiResponse<T> {
  data?: T;
  error?: ApiError;
  isLoading: boolean;
}

// Maze game types
export interface InitRequest {
  playerName: string;
  rowsColumns: number;
  playerId?: string; // Optional - for resuming existing session
}

export interface InitResponse {
  grid: number[][];
  playerData: PlayerData;
}

export interface MoveRequest {
  playerId: string; // Required for validating player
  x: number;
  y: number;
}

export interface MoveResponse {
  success: boolean;
  message?: string;
}

export interface PlayerData {
  playerId: string;
  matchId: string;
  name: string;
  currentPosition: { x: number; y: number };
  maze: number[][];
  finished: boolean;
  time: number; // elaspsed time in seconds since player started the maze
  createdAt: string;
  updatedAt: string;
}
