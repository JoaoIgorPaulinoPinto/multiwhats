import React from 'react'
import ReactDOM from 'react-dom/client'
import './globals.css'
import { App } from './App'
import { ErrorCatcher } from './error-catcher'
import { Hydrator } from './hydrator'
import { Providers } from './providers'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ErrorCatcher />
    <Providers>
      <Hydrator />
      <App />
    </Providers>
  </React.StrictMode>,
)
