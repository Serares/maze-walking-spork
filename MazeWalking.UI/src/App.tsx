import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { WeatherForecast } from './components/common/WeatherForecast'

function App() {
  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>MazeWalking React App</h1>
      <div className="card">
        <p>
          React + TypeScript + Vite connected to .NET Web API
        </p>
      </div>

      <WeatherForecast />

      <p className="read-the-docs" style={{ marginTop: '40px' }}>
        Edit <code>src/App.tsx</code> and save to test HMR
      </p>
    </>
  )
}

export default App
