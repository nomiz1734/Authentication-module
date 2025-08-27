import React, { useEffect, useState } from 'react'
import { getHome } from '../api'
import { useAuth } from '../context/AuthContext'

export default function Home() {
    const { accessToken, profile, signOut } = useAuth()
    const [msg, setMsg] = useState('')

    useEffect(() => {
        (async () => {
            try {
                const res = await getHome(accessToken)
                setMsg(res.message)
            } catch {
                setMsg('Error loading Homepage')
            }
        })()
    }, [accessToken])

    return (
        <div style={{ maxWidth: 520, margin: '40px auto' }}>
            <h2>Home</h2>
            <p>{msg}</p>
            {profile && <p>Welcome {profile.username} ({profile.type})</p>}
            <button onClick={signOut}>Logout</button>
        </div>
    )
}
