import React from 'react';
import './LoadingScreen.css';

interface LoadingScreenProps {
  message?: string;
  overlay?: boolean;
}

const LoadingScreen: React.FC<LoadingScreenProps> = ({ 
  message = "Loading...", 
  overlay = false 
}) => {
  return (
    <div className={`loading-screen ${overlay ? 'loading-overlay' : ''}`}>
      <div className="loading-particles">
        <div className="particle particle-1"></div>
        <div className="particle particle-2"></div>
        <div className="particle particle-3"></div>
        <div className="particle particle-4"></div>
      </div>
      <div className="loading-content">
        <div className="loading-spinner-ring">
          <div className="spinner-ring"></div>
          <div className="spinner-inner"></div>
        </div>
        <div className="loading-text">
          <h3>{message}</h3>
          <div className="loading-dots">
            <span></span>
            <span></span>
            <span></span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoadingScreen;
