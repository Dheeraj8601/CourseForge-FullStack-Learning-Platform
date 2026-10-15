
import axios from "axios";

// ==========================================
// 1. Axios Instance Configuration
// ==========================================

export const api = axios.create({
  baseURL:
    import.meta.env.VITE_API_URL ||
    "http://localhost:5080/api",
});

// ==========================================
// 2. Request Interceptor - JWT Token
// ==========================================

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("cf_token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// ==========================================
// 3. API Error Handling
// ==========================================

export const errorText = (error) => {
  if (typeof error.response?.data === "string") {
    return error.response.data;
  }

  return (
    error.response?.data?.message ||
    error.response?.data?.title ||
    error.message ||
    "Something went wrong"
  );
};

// ==========================================
// 4. User Session Management
// ==========================================

export const session = {
  // Get logged-in user details
  get: () => {
    try {
      return JSON.parse(
        localStorage.getItem("cf_user") || "null"
      );
    } catch {
      return null;
    }
  },

  // Save JWT token and user information
  set: (response) => {
    localStorage.setItem("cf_token", response.token);

    localStorage.setItem(
      "cf_user",
      JSON.stringify({
        name: response.name,
        email: response.email,
        role: response.role,
      })
    );

    // Notify React components about authentication change
    window.dispatchEvent(new Event("cf-auth"));
  },

  // Clear user session (logout)
  clear: () => {
    localStorage.removeItem("cf_token");
    localStorage.removeItem("cf_user");

    // Notify React components about logout
    window.dispatchEvent(new Event("cf-auth"));
  },
};
