import axios from 'axios';
import i18n from '../i18n';

// In production builds (npm run build → Tauri bundle), always point directly to
// the local backend. The __TAURI__ detection is unreliable at module load time
// depending on injection timing.
// In development (npm run dev), use VITE_API_URL env var or relative /api path.
const API_URL = import.meta.env.PROD
    ? 'http://127.0.0.1:45123/api'
    : (import.meta.env.VITE_API_URL || '/api');

export const client = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add token to requests
client.interceptors.request.use((config) => {
    try {
        // Add Language header
        config.headers['Accept-Language'] = i18n.language || 'en';

        const storage = localStorage.getItem('auth-storage');
        if (storage) {
            const { state } = JSON.parse(storage);
            if (state?.token) {
                config.headers.Authorization = `Bearer ${state.token}`;
            }
        }
    } catch (e) {
        console.error('Failed to parse auth token from localStorage', e);
    }
    return config;
});


// Optional: Add interceptors for global error handling
client.interceptors.response.use(
    (response) => response,
    (error) => {
        // Check for common errors
        if (error.response) {
            // Server responded with a status code outside the 2xx range
            console.error(`API Error: ${error.response.status} - ${error.response.data}`);
        } else if (error.request) {
            // The request was made but no response was received
            console.error('API Error: No response received', error.request);
        } else {
            // Something happened in setting up the request that triggered an Error
            console.error('API Error:', error.message);
        }
        return Promise.reject(error);
    }
);
