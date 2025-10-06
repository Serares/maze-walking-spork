import { useState, useEffect, useCallback } from 'react';
import { ApiError, ApiResponse } from '../types/api';

/**
 * Custom hook for making API calls with loading and error states
 * 
 * @param apiCall - The API function to call
 * @param immediate - Whether to call the API immediately on mount (default: true)
 * @returns Object containing data, error, loading state, and refetch function
 */
export function useApi<T>(
  apiCall: () => Promise<T>,
  immediate = true
): ApiResponse<T> & { refetch: () => Promise<void> } {
  const [data, setData] = useState<T | undefined>(undefined);
  const [error, setError] = useState<ApiError | undefined>(undefined);
  const [isLoading, setIsLoading] = useState<boolean>(immediate);

  const execute = useCallback(async () => {
    setIsLoading(true);
    setError(undefined);
    
    try {
      const result = await apiCall();
      setData(result);
      setError(undefined);
    } catch (err) {
      setError(err as ApiError);
      setData(undefined);
    } finally {
      setIsLoading(false);
    }
  }, [apiCall]);

  useEffect(() => {
    if (immediate) {
      execute();
    }
  }, [immediate, execute]);

  return {
    data,
    error,
    isLoading,
    refetch: execute,
  };
}
