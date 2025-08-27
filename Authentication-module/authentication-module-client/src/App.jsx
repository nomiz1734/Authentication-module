import React from 'react'
import { Routes, Route, Navigate, Link } from 'react-router-dom'
import Login from './pages/Login'
import Register from './pages/Register'
import Home from './pages/Home'
import ProtectedRoute from './components/ProtectedRoute'

export default function App() {
    return (
        <div style={{ fontFamily: 'system-ui', maxWidth: 520, margin: '40px auto' }}>
            <nav style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
                <Link to="/login">Login</Link>
                <Link to="/register">Register</Link>
                <Link to="/home">Home</Link>
            </nav>
            <Routes>
                <Route path="/" element={<Navigate to="/login" />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/home" element={<ProtectedRoute><Home /></ProtectedRoute>} />
            </Routes>
        </div>
    )
}
