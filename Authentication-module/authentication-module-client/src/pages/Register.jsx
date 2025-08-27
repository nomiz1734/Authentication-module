import React, { useState } from 'react'
import { register } from '../api'
import { useAuth } from '../context/AuthContext'

export default function Register() {
    const [username, setU] = useState('')
    const [email, setE] = useState('')
    const [password, setP] = useState('')
    const [type, setT] = useState('EndUser')
    const [err, setErr] = useState('')
    const { setAccessToken, setProfile } = useAuth()

    async function onSubmit(e) {
        e.preventDefault()
        setErr('')
        try {
            const res = await register({ username, email, password, type })
            setAccessToken(res.accessToken || res.AccessToken)
            setProfile({ username: res.username, email: res.email, type: res.type })
            window.location.href = '/home'
        } catch (e) {
            setErr(String(e.message || e))
        }
    }

    return (
        <form onSubmit={onSubmit} style={{ display: 'grid', gap: 8, maxWidth: 360, margin: '40px auto' }}>
            <h2>Register</h2>
            {err && <div style={{ color: 'red' }}>{err}</div>}
            <label>Username</label>
            <input value={username} onChange={e => setU(e.target.value)} required />

            <label>Email</label>
            <input type="email" value={email} onChange={e => setE(e.target.value)} required />

            <label>Password</label>
            <input type="password" value={password} onChange={e => setP(e.target.value)} required />

            <label>User Type</label>
            <select value={type} onChange={e => setT(e.target.value)}>
                <option>EndUser</option>
                <option>Admin</option>
                <option>Partner</option>
            </select>
            <button>Create Account</button>
        </form>
    )
}
