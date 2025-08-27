import React, { useState } from 'react'
import { login } from '../api'
import { useAuth } from '../context/AuthContext'

export default function Login() {
    const [usernameOrEmail, setU] = useState('')
    const [password, setP] = useState('')
    const [rememberMe, setR] = useState(true)
    const [type, setT] = useState('EndUser')
    const [err, setErr] = useState('')
    const { setAccessToken, setProfile } = useAuth()

    async function onSubmit(e) {
        e.preventDefault()
        setErr('')
        try {
            const res = await login({ usernameOrEmail, password, rememberMe, type })
            setAccessToken(res.accessToken || res.AccessToken)
            setProfile({ username: res.username, email: res.email, type: res.type })
            window.location.href = '/home'
        } catch (e) {
            setErr(String(e.message || e))
        }
    }

    return (
        <form onSubmit={onSubmit} style={{ display: 'grid', gap: 8, maxWidth: 360, margin: '40px auto' }}>
            <h2>Login</h2>
            {err && <div style={{ color: 'red' }}>{err}</div>}
            <label>Username or Email</label>
            <input value={usernameOrEmail} onChange={e => setU(e.target.value)} required />

            <label>Password</label>
            <input type="password" value={password} onChange={e => setP(e.target.value)} required />

            <label>User Type</label>
            <select value={type} onChange={e => setT(e.target.value)}>
                <option>EndUser</option>
                <option>Admin</option>
                <option>Partner</option>
            </select>

            <label><input type="checkbox" checked={rememberMe} onChange={e => setR(e.target.checked)} /> Remember Me</label>
            <button>Login</button>
        </form>
    )
}
