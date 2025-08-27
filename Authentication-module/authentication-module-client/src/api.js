const API = 'https://localhost:7241'

export async function register(payload) {
    const res = await fetch(`${API}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(payload)
    })
    if (!res.ok) throw new Error(await res.text())
    return res.json()
}

export async function login(payload) {
    const res = await fetch(`${API}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(payload)
    })
    if (!res.ok) throw new Error(await res.text())
    return res.json()
}

export async function refresh() {
    const res = await fetch(`${API}/api/auth/refresh`, {
        method: 'POST',
        credentials: 'include'
    })
    if (!res.ok) throw new Error(await res.text())
    return res.json()
}

export async function logout(accessToken) {
    const res = await fetch(`${API}/api/auth/logout`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${accessToken}` },
        credentials: 'include'
    })
    if (!res.ok) throw new Error(await res.text())
    return true
}

export async function getHome(accessToken) {
    const res = await fetch(`${API}/api/home`, {
        headers: { 'Authorization': `Bearer ${accessToken}` },
        credentials: 'include'
    })
    if (!res.ok) throw new Error(await res.text())
    return res.json()
}
