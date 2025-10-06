import type { WeatherForecast, ApiError } from '../types/api';

// Base API configuration
const API_BASE_URL = '/api';

/**
 * Generic fetch wrapper with error handling
 */
async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      ...options,
    });

    if (!response.ok) {
      const error: ApiError = {
        message: `HTTP error! status: ${response.status}`,
        status: response.status,
      };
      
      try {
        const errorData = await response.json();
        error.details = errorData;
      } catch {
        // If response body is not JSON, use status text
        error.message = response.statusText || error.message;
      }
      
      throw error;
    }

    return await response.json();
  } catch (error) {
    if ((error as ApiError).status) {
      throw error;
    }
    
    // Network or other errors
    throw {
      message: error instanceof Error ? error.message : 'An unknown error occurred',
      status: 0,
      details: error,
    } as ApiError;
  }
}

/**
 * API client service
 */
export const apiClient = {
  /**
   * Get weather forecast
   */
  async getWeatherForecast(): Promise<WeatherForecast[]> {
    return fetchApi<WeatherForecast[]>('/weatherforecast');
  },

  // Add more API methods here as your backend expands
  // Example:
  // async getMazeData(id: string): Promise<MazeData> {
  //   return fetchApi<MazeData>(`/maze/${id}`);
  // },
};

export default apiClient;
