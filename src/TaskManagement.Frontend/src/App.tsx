import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { TaskProvider } from './contexts/TaskContext';
import Login from './components/Login';
import KanbanBoard from './components/KanbanBoard';
import Logo from './components/Logo';
import './App.css';

function AppContent() {
  const { user, loading, logout } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Loading...</div>
      </div>
    );
  }

  if (!user) {
    return <Login />;
  }

  return (
    <TaskProvider>
      <div className="app-container">
        <header className="app-header">
          <div className="header-content">
            <div className="app-logo-container">
              <Logo className="app-logo" variant="colored" size="large" />
            </div>
            <div className="user-info">
              <span className="welcome-text">Welcome, {user.name || user.email}</span>
              <button onClick={logout} className="logout-btn">
                Logout
              </button>
            </div>
          </div>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/dashboard" element={<KanbanBoard />} />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </main>
      </div>
    </TaskProvider>
  );
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </Router>
  );
}

export default App;
