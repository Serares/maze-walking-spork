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
}

export interface InitResponse {
  grid: number[][];
}

export interface MoveRequest {
  x: number;
  y: number;
}

export interface MoveResponse {
  success: boolean;
  message?: string;
}
