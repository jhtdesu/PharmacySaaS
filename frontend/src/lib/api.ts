import axios from 'axios';

export const api = axios.create({
    baseURL: 'http://localhost:5000/api', 
    headers: {
        'Content-Type': 'application/json'
    }
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('jwt_token');
    
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

api.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
            
            originalRequest._retry = true; 

            try {
                const refreshToken = localStorage.getItem('refresh_token');
                const jwtToken = localStorage.getItem('jwt_token');

                if (!refreshToken) {
                    throw new Error("No refresh token available.");
                }

                const refreshResponse = await axios.post('http://localhost:5000/api/auth/refresh', {
                    accessToken: jwtToken,
                    refreshToken: refreshToken
                });

                const newJwt = refreshResponse.data.data.accessToken;
                const newRefresh = refreshResponse.data.data.refreshToken;

                localStorage.setItem('jwt_token', newJwt);
                localStorage.setItem('refresh_token', newRefresh);

                originalRequest.headers.Authorization = `Bearer ${newJwt}`;
                return api(originalRequest);

            } catch (refreshError) {
                console.error("Session expired. Please log in again.", refreshError);
                localStorage.removeItem('jwt_token');
                localStorage.removeItem('refresh_token');
                
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);