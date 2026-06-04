import { defineStore } from 'pinia';
import apiClient from '@/services/apiClient';
import { useAuthStore } from './AuthStore';

export const useDashboardStore = defineStore('dashboard', {
  state: () => ({
    superAdminDashboard: null,
    loading: false,
    error: null,
  }),

  actions: {
    async fetchSuperAdminDashboard() {
      this.loading = true;
      this.error = null;
      
      try {
        // ✅ Make sure we have a valid token
        const authStore = useAuthStore();
        
        if (!authStore.accessToken) {
          throw new Error('No authentication token found. Please login again.');
        }
        
        console.log('Fetching SuperAdmin dashboard...');
        console.log('Token exists:', !!authStore.accessToken);
        
        // ✅ The token should already be in apiClient headers from AuthStore
        // But let's verify
        const response = await apiClient.get('/Dashboard/superadmin');
        
        console.log('Response status:', response.status);
        console.log('Response data:', response.data);
        
        // ✅ Handle the response format correctly
        // Your controller returns the value directly, not wrapped in Result
        if (response.data) {
          this.superAdminDashboard = response.data;
          return response.data;
        }
        
        throw new Error('Invalid response format');
      } catch (error) {
        console.error('API Error Details:', {
          message: error.message,
          response: error.response?.data,
          status: error.response?.status,
          headers: error.response?.headers
        });
        
        if (error.response?.status === 401) {
          this.error = 'Session expired. Please login again.';
          // Optionally redirect to login
          const authStore = useAuthStore();
          authStore.logout();
        } else if (error.response?.status === 403) {
          this.error = 'Access denied. SuperAdmin role required.';
        } else {
          this.error = error.response?.data?.message || error.message || 'Failed to load dashboard';
        }
        
        throw new Error(this.error);
      } finally {
        this.loading = false;
      }
    },
  },
});