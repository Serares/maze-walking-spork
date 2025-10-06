// API response types based on MazeWalking.Web endpoints

export interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

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
