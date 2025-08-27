import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react'
import basicSsl from '@vitejs/plugin-basic-ssl'
//import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react(), basicSsl()],
    server: {
        port: 5173,
        https: true,
        proxy: {
            '/api': {
                target: 'https://localhost:7241',
                secure: false,                  
                changeOrigin: false
            }
        }
    }
})