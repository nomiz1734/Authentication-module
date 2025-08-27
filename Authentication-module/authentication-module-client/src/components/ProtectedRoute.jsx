import React from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function ProtectedRoute({ children }) {
    const { accessToken, loading } = useAuth()
    if (loading) return <div>Checking login...</div>
    if (!accessToken) return <Navigate to="/login" />
    return children
}
