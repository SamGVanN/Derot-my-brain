import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5077/api';

export const client = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add token to requests
client.interceptors.request.use((config) => {
    try {
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
