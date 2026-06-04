import { defineStore } from "pinia";
import { ref, computed } from "vue";
import apiClient from "@/services/apiClient";
import { jwtDecode } from "jwt-decode";
import router from "@/router/Index";

export const useAuthStore = defineStore("auth", () => {
  // =====================================================
  // STATE
  // =====================================================

  const user = ref(null);
  const accessToken = ref(localStorage.getItem("accessToken"));
  const refreshToken = ref(localStorage.getItem("refreshToken"));

  const isAuthenticated = computed(() => !!accessToken.value);

  // =====================================================
  // RESTORE SESSION ON APP START
  // =====================================================

  if (accessToken.value) {
    apiClient.defaults.headers.common["Authorization"] = `Bearer ${accessToken.value}`;

    try {
      const storedUser = localStorage.getItem("user");
      user.value = storedUser ? JSON.parse(storedUser) : null;
    } catch {
      user.value = null;
    }
  }

  // =====================================================
  // LOGIN
  // =====================================================

  const login = async (email, password, latitude, longitude) => {
    try {
      const response = await apiClient.post("/Auth/login", {
        email,
        password,
        currentLatitude: latitude,
        currentLongitude: longitude,
        // currentLatitude: 0.983978,
        // currentLongitude: 34.98466,
      });
      console.log("latitude: ", latitude);
      console.log("longitude: ", longitude);
      // Because backend returns Result<T>
      const { isSuccess, value, error } = response.data;
      if (!isSuccess) {
        console.log("Backend error:", error);
        return { success: false, error };
      }

      if (!isSuccess) {
        return { success: false, error };
      }

      const { user: userData, accessToken: token, refreshToken: refToken } = value;

      // Save state
      user.value = userData;
      accessToken.value = token;
      refreshToken.value = refToken;

      // Save to localStorage
      localStorage.setItem("user", JSON.stringify(userData));
      localStorage.setItem("accessToken", token);
      localStorage.setItem("refreshToken", refToken);

      // Attach token to axios
      apiClient.defaults.headers.common["Authorization"] = `Bearer ${token}`;

      return { success: true };
    } catch (err) {
      return {
        success: false,
        error: err.response?.data?.error || err.response?.data?.message || "Login failed",
      };
    }
  };

  // =====================================================
  // LOGOUT
  // =====================================================

  const logout = async () => {
    try {
      await apiClient.post("/Auth/logout");
    } catch (err) {
      console.warn("Logout request failed:", err.message);
    } finally {
      // Always clear everything
      user.value = null;
      accessToken.value = null;
      refreshToken.value = null;

      localStorage.clear();

      delete apiClient.defaults.headers.common["Authorization"];

      router.push("/login");
    }
  };

  // =====================================================
  // GEOLOCATION
  // =====================================================

  const getCurrentPosition = () => {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error("Geolocation not supported"));
      } else {
        navigator.geolocation.getCurrentPosition(resolve, reject, {
          enableHighAccuracy: true,
          timeout: 15000,
          maximumAge: 0,
        });
      }
    });
  };

  // =====================================================
  // TOKEN EXPIRY CHECK
  // =====================================================

  const isTokenExpired = () => {
    if (!accessToken.value) return true;

    try {
      const decoded = jwtDecode(accessToken.value);
      return decoded.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  };

  // =====================================================
  // USER ROLE
  // =====================================================

  const userRole = computed(() => {
    if (!accessToken.value) return null;

    try {
      const decoded = jwtDecode(accessToken.value);

      return decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    } catch {
      return null;
    }
  });

  // =====================================================
  // RETURN
  // =====================================================

  return {
    user,
    accessToken,
    refreshToken,
    isAuthenticated,
    userRole,
    login,
    logout,
    isTokenExpired,
    getCurrentPosition,
  };
});
