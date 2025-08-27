import React, { createContext, useContext, useEffect, useState } from 'react'
import { refresh, logout } from '../api'

const Ctx = createContext(null)

export function AuthProvider({ children }) {
    const [accessToken, setAccessToken] = useState('')
    const [profile, setProfile] = useState(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        (async () => {
            try {
                const res = await refresh()
                setAccessToken(res.accessToken || res.AccessToken)
                setProfile({ username: res.username, email: res.email, type: res.type })
            } catch { }
            setLoading(false)
        })()
    }, [])

    const value = {
        accessToken, setAccessToken,
        profile, setProfile,
        loading,
        async signOut() {
            try { await logout(accessToken) } catch { }
            setAccessToken('')
            setProfile(null)
        }
    }
    return <Ctx.Provider value={value}>{children}</Ctx.Provider>
}

export function useAuth() { return useContext(Ctx) }
