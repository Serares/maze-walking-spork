import type { ApiError, InitRequest, InitResponse, MoveRequest, MoveResponse } from '../types/api';

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
   * Initialize a new maze game
   */
  async initMaze(request: InitRequest): Promise<InitResponse> {
    return fetchApi<InitResponse>('/init', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  /**
   * Move player to new position
   */
  async movePlayer(request: MoveRequest): Promise<MoveResponse> {
    return fetchApi<MoveResponse>('/move', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },
};

export default apiClient;
