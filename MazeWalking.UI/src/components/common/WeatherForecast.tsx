import { useCallback } from 'react';
import { useApi } from '../../hooks/useApi';
import apiClient from '../../services/api';
import { WeatherForecast as WeatherForecastType } from '../../types/api';

/**
 * WeatherForecast component - demonstrates API integration with the .NET backend
 */
export function WeatherForecast() {
  // Use the custom hook to fetch weather data
  const { data, error, isLoading, refetch } = useApi(
    useCallback(() => apiClient.getWeatherForecast(), []),
    true // Fetch immediately on mount
  );

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <h2>Weather Forecast</h2>
      <p style={{ color: '#666' }}>
        Data from MazeWalking.Web API (demonstrating frontend-backend integration)
      </p>

      <button 
        onClick={refetch} 
        disabled={isLoading}
        style={{
          padding: '10px 20px',
          marginBottom: '20px',
          backgroundColor: '#007bff',
          color: 'white',
          border: 'none',
          borderRadius: '4px',
          cursor: isLoading ? 'not-allowed' : 'pointer',
          opacity: isLoading ? 0.6 : 1,
        }}
      >
        {isLoading ? 'Loading...' : 'Refresh Data'}
      </button>

      {error && (
        <div style={{
          padding: '15px',
          backgroundColor: '#f8d7da',
          color: '#721c24',
          borderRadius: '4px',
          marginBottom: '20px'
        }}>
          <strong>Error:</strong> {error.message}
          {error.details && (
            <pre style={{ marginTop: '10px', fontSize: '12px' }}>
              {JSON.stringify(error.details, null, 2)}
            </pre>
          )}
        </div>
      )}

      {isLoading && !data && (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <div style={{ 
            fontSize: '18px', 
            color: '#666',
            animation: 'pulse 1.5s ease-in-out infinite'
          }}>
            Loading weather data...
          </div>
        </div>
      )}

      {data && (
        <div style={{
          display: 'grid',
          gap: '15px',
          gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))'
        }}>
          {data.map((forecast: WeatherForecastType, index: number) => (
            <div
              key={index}
              style={{
                padding: '20px',
                backgroundColor: '#f8f9fa',
                borderRadius: '8px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                transition: 'transform 0.2s',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-2px)';
                e.currentTarget.style.boxShadow = '0 4px 8px rgba(0,0,0,0.15)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
              }}
            >
              <h3 style={{ marginTop: 0, color: '#333' }}>
                {new Date(forecast.date).toLocaleDateString('en-US', {
                  weekday: 'long',
                  month: 'short',
                  day: 'numeric'
                })}
              </h3>
              <div style={{ fontSize: '14px', color: '#666' }}>
                <p><strong>Temperature:</strong> {forecast.temperatureC}°C / {forecast.temperatureF}°F</p>
                <p><strong>Summary:</strong> {forecast.summary}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
